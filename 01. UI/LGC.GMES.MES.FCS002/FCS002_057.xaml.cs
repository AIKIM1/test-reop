/*************************************************************************************
 Created Date : 2021.01.
      Creator : 
   Decription : 충방전 실적관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.01. DEVELOPER : Initial Created.
  2021.08.18  강동희 : 특이사항 입/출력 기능 추가
  
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_057 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private DataTable[] dtHeader = new DataTable[4];
        public FCS002_057()
        {
            InitializeComponent();

            //DEGAS 전 1차 충전
            dtHeader[0] = SetGridHeader(dgFormBFD, "A", null).DefaultView.ToTable(false, new string[] { "DFCT_CODE" }); //Degas 전 직행
            dtHeader[2] = SetGridHeader(dgFormBF, "A", "B").DefaultView.ToTable(false, new string[] { "DFCT_CODE" }); //Degas 전 재작업 일지 Header Setting

            //DEGAS 후 2차 충전
            dtHeader[1] = SetGridHeader(dgFormAFD, "B", null).DefaultView.ToTable(false, new string[] { "DFCT_CODE" }); //Degas 후 직행
            dtHeader[3] = SetGridHeader(dgFormAF, "B", "B").DefaultView.ToTable(false, new string[] { "DFCT_CODE" }); //Degas 후 재작업 일지 Header Setting

            InitControl();
            InitCombo();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            _combo.SetCombo(cboShift, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "FORM_SHIFT");
            _combo.SetCombo(cboShift2, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "FORM_SHIFT");
            _combo.SetCombo(cboEQSGID, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE");
            _combo.SetCombo(cboEQSGID2, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE");

        }
        private void InitControl()
        {
            dtpWorkDate.SelectedDateTime = DateTime.Now;
        }
        #endregion

        #region Method

        private DataTable SetGridHeader(C1DataGrid dg, string sEqpKindCd, string sDefectKind)
        {
            Util.gridClear(dg); //Grid clear
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("DFCT_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["DFCT_GR_TYPE_CODE"] = sEqpKindCd;
            dr["DFCT_TYPE_CODE"] = sDefectKind;
            dr["USE_FLAG"] = "Y";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT_MB", "RQSTDT", "RSLTDT", dtRqst);

            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                string sDefCode = dtRslt.Rows[i]["DFCT_CODE"].ToString();
                string sGrpName = dtRslt.Rows[i]["GROUP_NAME"].ToString();
                string SDefName = dtRslt.Rows[i]["DFCT_NAME"].ToString();
                string sTag = Util.NVC(dtRslt.Rows[i]["DFCT_TYPE_CODE"]);

                dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = sDefCode,
                    Header = new string[] { sGrpName, SDefName }.ToList<string>(),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath(sDefCode),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Format = "#,##0",
                    Tag = sTag
                });
            }
            return dtRslt;
        }
        private void GetListBF()
        {
            try
            {
                Util.gridClear(dgFormBFD);
                Util.gridClear(dgFormBF);

                GetSummaryData("A");
                GetRemarkData("A"); //2021.08.18 특이사항 입/출력 기능 추가
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        private void GetListAF()
        {
            try
            {
                Util.gridClear(dgFormAFD);
                Util.gridClear(dgFormAF);
                GetSummaryData("B");
                GetRemarkData("B"); //2021.08.18 특이사항 입/출력 기능 추가
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetSummaryData(string WRKLOG_TYPE_CODE)
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("WORK_DATE", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WRKLOG_TYPE_CODE"] = WRKLOG_TYPE_CODE;
                if (WRKLOG_TYPE_CODE.Equals("A"))
                {
                    dr["WORK_DATE"] = dtpWorkDate2.SelectedDateTime.ToString("yyyy-MM-dd");
                    dr["SHFT_ID"] = Util.GetCondition(cboShift2, bAllNull: true);
                    if (!string.IsNullOrEmpty(Util.GetCondition(cboEQSGID, bAllNull: true)))
                    {
                        dr["EQSGID"] = Util.GetCondition(cboEQSGID, bAllNull: true);
                    }
                }
                else
                {
                    dr["WORK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd");
                    dr["SHFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
                    if (!string.IsNullOrEmpty(Util.GetCondition(cboEQSGID2, bAllNull: true)))
                    {
                        dr["EQSGID"] = Util.GetCondition(cboEQSGID2, bAllNull: true);
                    }
                }
               
                dtRqst.Rows.Add(dr);
                DataSet InDataSet = new DataSet();
                InDataSet.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_FORMATION_PERF", "INDATA", "OUT_DIR,OUT_DIR_DETAIL,OUT_RWK,OUT_RWK_DETAIL", InDataSet);

                DataTable dtDir = dsRslt.Tables["OUT_DIR"];
                DataTable dtDirDetail = dsRslt.Tables["OUT_DIR_DETAIL"];
                DataTable dtRwk = dsRslt.Tables["OUT_RWK"];
                DataTable dtRwkDetail = dsRslt.Tables["OUT_RWK_DETAIL"];

                if (WRKLOG_TYPE_CODE.Equals("A")) // 1차충전 실적 조회
                {
                    for (int i = 0; i < dtHeader[0].Rows.Count; i++) // 컬럼추가 - 1차 충전 직행 불량 코드
                    {
                        dtDir.Columns.Add(Util.NVC(dtHeader[0].Rows[i]["DFCT_CODE"]));
                    }
                    for (int i = 0; i < dtHeader[2].Rows.Count; i++) // 컬럼추가 - 1차 충전 재작업 불량 코드
                    {
                        dtRwk.Columns.Add(Util.NVC(dtHeader[2].Rows[i]["DFCT_CODE"]));
                    }

                    SetGridPerf(dgFormBFD, dtDir, dtDirDetail);
                    SetGridPerf(dgFormBF, dtRwk, dtRwkDetail);
                }
                else  // 2차 충전 실적 조회
                {
                    for (int i = 0; i < dtHeader[1].Rows.Count; i++)
                    {
                        dtDir.Columns.Add(Util.NVC(dtHeader[1].Rows[i]["DFCT_CODE"])); // 컬럼추가 - 2차 충전 직행 불량 코드
                    }
                    for (int i = 0; i < dtHeader[3].Rows.Count; i++)
                    {
                        dtRwk.Columns.Add(Util.NVC(dtHeader[3].Rows[i]["DFCT_CODE"])); // 컬럼추가 - 2차 충전 재작업 불량 코드
                    }
                    SetGridPerf(dgFormAFD, dtDir, dtDirDetail);
                    SetGridPerf(dgFormAF, dtRwk, dtRwkDetail);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void SetGridPerf(C1DataGrid dg, DataTable dt, DataTable dtDetail)
        {
            try
            {
                foreach (DataRow drDetail in dtDetail.Rows)
                {
                    int iRow = -1;
                    int iCol = -1;

                    //LOT TYPE 추가 시 수정
                    string sCurrRow = "";
                    sCurrRow = (Util.NVC(drDetail["LOT_COMENT"]) + "_" + Util.NVC(drDetail["EQSGID"]) + "_" + Util.NVC(drDetail["PROD_LOTID"])
                        + "_" + Util.NVC(drDetail["LOTTYPE"]));

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        //LOT TYPE 추가 시 수정
                        string sRow = "";
                        sRow = (Util.NVC(dt.Rows[i]["LOT_COMENT"]) + "_" + Util.NVC(dt.Rows[i]["EQSGID"]) + "_" + Util.NVC(dt.Rows[i]["PROD_LOTID"])
                            + "_" + Util.NVC(dt.Rows[i]["LOTTYPE"]));

                        if (sCurrRow.Equals(sRow))
                        {
                            iRow = i;
                            iCol = dt.Columns.IndexOf(Util.NVC(drDetail["DFCT_CODE"]));
                            if (iRow != -1 && iCol != -1)
                            {
                                dt.Rows[iRow][iCol] = Util.NVC(drDetail["BAD_SUBLOT_COUNT"]);
                                dt.Rows[iRow]["LOT_COMENT"] = Util.NVC(drDetail["LOT_COMENT"]);
                                break;
                            }
                        }
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    AddFooterSum(dt);
                    Util.GridSetData(dg, dt, this.FrameOperation, true);
                }
                string[] sColumnName = new string[] { "MDLLOT_ID", "EQSGID", "PROD_LOTID" };
                _Util.SetDataGridMergeExtensionCol(dg, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void AddFooterSum(DataTable dt)
        {
            DataRow dr = dt.NewRow();
            dr[dt.Columns.IndexOf("INPUT_SUBLOT_QTY") - 1] = ObjectDic.Instance.GetObjectName("SUM");

            for (int i = dt.Columns.IndexOf("INPUT_SUBLOT_QTY"); i < dt.Columns.Count; i++)
            {
                int totalCnt = 0;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    totalCnt += Util.NVC_Int(dt.Rows[j][i]);
                }
                if (totalCnt != 0) dr[dt.Columns[i].ColumnName] = totalCnt;
            }
            dt.Rows.Add(dr);
        }
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
      
        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_054_DFCT_CELL_LIST window = sender as FCS002_054_DFCT_CELL_LIST;
        }

        //2021.08.18 특이사항 입/출력 기능 추가 START
        private void GetRemarkData(string WRKLOG_TYPE_CODE)
        {
            try
            {
                if (WRKLOG_TYPE_CODE.Equals("A"))
                {
                    txtRemarkA.Text = string.Empty;
                }
                else
                {
                    txtRemarkB.Text = string.Empty;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("WRK_DATE", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRKLOG_TYPE_CODE"] = WRKLOG_TYPE_CODE;
                if (WRKLOG_TYPE_CODE.Equals("A"))
                {
                    dr["WRK_DATE"] = dtpWorkDate2.SelectedDateTime.ToString("yyyyMMdd");
                    dr["SHFT_ID"] = Util.GetCondition(cboShift2, bAllNull: true);
                }
                else
                {
                    dr["WRK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
                    dr["SHFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RSLT_REMARK_MB", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    if (WRKLOG_TYPE_CODE.Equals("A"))
                    {
                        txtRemarkA.Text = Util.NVC(dtRslt.Rows[0]["REMARKS_CNTT"].ToString());
                    }
                    else
                    {
                        txtRemarkB.Text = Util.NVC(dtRslt.Rows[0]["REMARKS_CNTT"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //2021.08.18 특이사항 입/출력 기능 추가 END

        #endregion

        #region Event
        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                string tag = Util.NVC(e.Cell.Column.Tag);

                if (tag.Equals("A"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                }
                else if (!string.IsNullOrEmpty(tag))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }

                if (e.Cell.Row.Index == dataGrid.Rows.Count() - 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
            }));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetListAF();
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetListBF();
        }


        private void dgDirList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            // Header에추가된 불량코드들은 불량그룹코드 태그로 가지고 있음
            if (cell.Text != datagrid.CurrentColumn.Header.ToString() && !string.IsNullOrEmpty(Util.NVC(cell.Column.Tag)))
            {
                FCS002_054_DFCT_CELL_LIST wndPopup = new FCS002_054_DFCT_CELL_LIST();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    int row = cell.Row.Index;
                    object[] Parameters = new object[12];


                    if (row == datagrid.Rows.Count - 1)
                    {
                        for (int i = 2; i <= 7; i++)
                        {
                            Parameters[i] = "";
                        }
                    }
                    else
                    {
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "MDLLOT_ID")); //MDLLOT_ID
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQSGID")); //EQSGID
                        Parameters[4] = ""; //EQPTID
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROD_LOTID")); //PROD_LOTID
                        Parameters[6] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "LOTTYPE")); // LOTTYPE
                    }
                    if (datagrid.Name.Equals("dgFormBFD"))
                    {
                        Parameters[0] = dtpWorkDate2.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                        Parameters[1] = Util.GetCondition(cboShift2, bAllNull: true); //SHFT_ID
                        Parameters[8] = "PRE_DEGAS_FORMEQPT_SELECTOR_MOVE";
                    }
                    else
                    {
                        Parameters[0] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                        Parameters[1] = Util.GetCondition(cboShift, bAllNull: true); //SHFT_ID
                        Parameters[8] = "AFTER_DEGAS_FORMEQPT_SELECTOR_MOVE";
                    }
                    Parameters[7] = cell.Column.Name; // DFCT_CODE
                    Parameters[9] = "N"; // RWK_FLAG
                    Parameters[10] = ""; // ROUT_TYPE_CODE
                    Parameters[11] = ""; // LOTCOMENT, 2021-04-23 재작업일지일 경우 이전 불량명 추가 
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    wndPopup.Closed += new EventHandler(wndPopup_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void dgRwkList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            // Header에추가된 불량코드들은 불량그룹코드 태그로 가지고 있음
            if (cell.Text != datagrid.CurrentColumn.Header.ToString() && !string.IsNullOrEmpty(Util.NVC(cell.Column.Tag)))
            {
                FCS002_054_DFCT_CELL_LIST wndPopup = new FCS002_054_DFCT_CELL_LIST();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    int row = cell.Row.Index;
                    object[] Parameters = new object[12];

                    if (row == datagrid.Rows.Count - 1)
                    {
                        for (int i = 2; i <= 7; i++)
                        {
                            Parameters[i] = "";
                        }
                    }
                    else
                    {
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "MDLLOT_ID")); //MDLLOT_ID
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQSGID")); //EQSGID
                        Parameters[4] = ""; //EQPTID
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROD_LOTID")); //PROD_LOTID
                        Parameters[6] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "LOTTYPE")); // LOTTYPE
                    }
                    Parameters[7] = cell.Column.Name; // DFCT_CODE
                    Parameters[9] = "Y"; // RWK_FLAG

                    // 2021-04-23 재작업일지일 경우 이전 불량명 추가 
                    Parameters[11] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PRE_DFCT_CODE")); //LOTCOMENT, 이전불량명

                    if (datagrid.Name.Equals("dgFormBF"))
                    {
                        Parameters[0] = dtpWorkDate2.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                        Parameters[1] = Util.GetCondition(cboShift2, bAllNull: true); //SHFT_ID
                        Parameters[8] = "PRE_DEGAS_FORMEQPT_SELECTOR_MOVE";
                        Parameters[10] = "D,R,H"; // ROUT_TYPE_CODE
                    }
                    else
                    {
                        Parameters[0] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                        Parameters[1] = Util.GetCondition(cboShift, bAllNull: true); //SHFT_ID
                        Parameters[8] = "AFTER_DEGAS_FORMEQPT_SELECTOR_MOVE";
                        Parameters[10] = "C,I,K"; // ROUT_TYPE_CODE
                    }

                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    wndPopup.Closed += new EventHandler(wndPopup_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }
        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        //2021.08.18 특이사항 입/출력 기능 추가 START
        private void btnSaveB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0214", (result) =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("WRK_DATE", typeof(string));
                        dtRqst.Columns.Add("SHFT_ID", typeof(string));
                        dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                        dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["WRK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
                        dr["SHFT_ID"] = Util.GetCondition(cboShift2);
                        dr["WRKLOG_TYPE_CODE"] = "B";
                        dr["REMARKS_CNTT"] = Util.GetCondition(txtRemarkB);
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RSLT_REMARKS_MB", "INDATA", "OUTDATA", dtRqst);

                        if (Util.NVC_Int(dtRslt.Rows[0]["RETVAL"]) == 0)
                        {
                            Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
                        }
                        else
                        {
                            Util.MessageInfo("FM_ME_0213"); //저장실패하였습니다.
                        }
                        GetRemarkData("B");
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //2021.08.18 특이사항 입/출력 기능 추가 END

        //2021.08.18 특이사항 입/출력 기능 추가 START
        private void btnSaveA_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0214", (result) =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("WRK_DATE", typeof(string));
                        dtRqst.Columns.Add("SHFT_ID", typeof(string));
                        dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                        dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["WRK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
                        dr["SHFT_ID"] = Util.GetCondition(cboShift);
                        dr["WRKLOG_TYPE_CODE"] = "A";
                        dr["REMARKS_CNTT"] = Util.GetCondition(txtRemarkA);
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RSLT_REMARKS_MB", "INDATA", "OUTDATA", dtRqst);

                        if (Util.NVC_Int(dtRslt.Rows[0]["RETVAL"]) == 0)
                        {
                            Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
                        }
                        else
                        {
                            Util.MessageInfo("FM_ME_0213"); //저장실패하였습니다.
                        }
                        GetRemarkData("A");
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //2021.08.18 특이사항 입/출력 기능 추가 END
        #endregion


    }
}
