/*************************************************************************************
 Created Date : 2021.04.11
      Creator : 
   Decription : 저전압, 용량배출 실적관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.01. DEVELOPER : Initial Created.
  2022.07.25  KDH    : C20220603-000198_저전압 1차/2차 선택 기능 추가
  2023.01.26  권혜정 : 저전압 실적 관리 상세 팝업 > 합계 부분 생산라인(EQSQID) 조건 추가
  2023.02.21  권혜정 : 저전압 실적 관리 상세 팝업 > Parameters[11] = PRE_DFCT_CODE로 변경, 재작업 불량 MouseDoubleClick 이벤트 추가
  2023.02.22  조영대 : 2차 저전압 조회 시 불량 CELL 목록 조회 인수 FORM_RSLT_SUM_GR_CODE 분기, LoadingIndicator 적용
  2023.03.27  조영대 : 불량명 컬럼헤더 다국어 처리 오류 수정, 필터 적용시 합계 수정
  2023.04.17  하유승 : MultiSelectionBox 추가
  2023.07.14  이정미 : Parameters 변수 배열 크기 수정 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_110 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtHeader = new DataTable();

        bool bUseFlag = false;

        public FCS001_110()
        {
            InitializeComponent();
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
            CommonCombo_Form _combo = new CommonCombo_Form();
            _combo.SetCombo(cboShift, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_SHIFT");
            _combo.SetCombo(cboEQSGID, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");
        }

        private void InitControl()
        {
            dtpWorkDate.SelectedDateTime = DateTime.Now;
        }

        #endregion

        #region Method
        private void SetGridHeader(C1DataGrid dg)
        {
            Util.gridClear(dg); //Grid clear
            DataSet dsDirectInfo = new DataSet();

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));
            dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["USE_FLAG"] = "Y";
            dr["DFCT_GR_TYPE_CODE"] = "G";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT", "RQSTDT", "RSLTDT", dtRqst);

            DataGridAggregate.SetAggregateFunctions(dg.Columns["LOT_ATTR"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplateSum"] as DataTemplate } });
            DataGridAggregate.SetAggregateFunctions(dg.Columns["INPUT_SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
            DataGridAggregate.SetAggregateFunctions(dg.Columns["GOOD_SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
            DataGridAggregate.SetAggregateFunctions(dg.Columns["NG_SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });

            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                string sDefCode = dtRslt.Rows[i]["DFCT_CODE"].ToString();
                string sGrpName = dtRslt.Rows[i]["GROUP_NAME"].ToString();
                string SDefName = dtRslt.Rows[i]["DFCT_NAME"].ToString();
                string sTag = Util.NVC(dtRslt.Rows[i]["DFCT_TYPE_CODE"]);

                dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = sDefCode,
                    //[*] 문자 선행일때 다국어 처리 스킵
                    Header = new string[] { sGrpName, "[*]" + SDefName }.ToList<string>(),
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

                DataGridAggregate.SetAggregateFunctions(dg.Columns[sDefCode], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
            }
            dtHeader = dtRslt.DefaultView.ToTable(false, new string[] { "DFCT_CODE" });
        }

        private void GetList()
        {
            try
            {
                Util.gridClear(dgDIR);
                Util.gridClear(dgRework);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WORK_DATE", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
                dr["WORK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd");

                dr["LOTTYPE"] = cboLotType.GetBindValue();

                //C20220603-000198_저전압 1차/2차 선택 기능 추가 START
                if (bUseFlag)
                {
                    if (rdoLowVolt.IsChecked.Equals(true))
                    {
                        dr["WRKLOG_TYPE_CODE"] = "G"; //1차 저전압 
                    }
                    else
                    {
                        dr["WRKLOG_TYPE_CODE"] = "G2"; //2차 저전압 
                    }
                }
                else
                {
                    dr["WRKLOG_TYPE_CODE"] = "G"; //1차 저전압 
                }
                //C20220603-000198_저전압 1차/2차 선택 기능 추가 END

                if (!string.IsNullOrEmpty(Util.GetCondition(cboEQSGID, bAllNull: true)))
                {
                    dr["EQSGID"] = Util.GetCondition(cboEQSGID, bAllNull: true);
                }

                dtRqst.Rows.Add(dr);

                DataSet InDataSet = new DataSet();
                InDataSet.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                
                new ClientProxy().ExecuteService_Multi("BR_GET_FORMATION_PERF", "INDATA", "OUT_DIR,OUT_DIR_DETAIL,OUT_RWK,OUT_RWK_DETAIL", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        DataTable dtDir = bizResult.Tables["OUT_DIR"];
                        DataTable dtDirDetail = bizResult.Tables["OUT_DIR_DETAIL"];
                        DataTable dtRwk = bizResult.Tables["OUT_RWK"];
                        DataTable dtRwkDetail = bizResult.Tables["OUT_RWK_DETAIL"];

                        for (int i = 0; i < dtHeader.Rows.Count; i++)
                        {
                            dtDir.Columns.Add(Util.NVC(dtHeader.Rows[i]["DFCT_CODE"]));
                            dtRwk.Columns.Add(Util.NVC(dtHeader.Rows[i]["DFCT_CODE"]));
                        }
                        SetGridPerf(dgDIR, dtDir, dtDirDetail);
                        SetGridPerf(dgRework, dtRwk, dtRwkDetail);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, InDataSet);


  
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //실적 확정 미사용

        /*
    private bool CheckPerfConfirm()
    {
        DataTable dtRqst = new DataTable();
        dtRqst.TableName = "RQSTDT";
        dtRqst.Columns.Add("WRK_DATE", typeof(string));
        dtRqst.Columns.Add("EQSGID", typeof(string));
        dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
        dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
        dtRqst.Columns.Add("SHFT_ID", typeof(string));
        dtRqst.Columns.Add("EQPTID", typeof(string));

        DataRow dr = dtRqst.NewRow();

        dr["WRK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
        dr["EQSGID"] = "ALL";
        dr["MDLLOT_ID"] = "ALL";
        dr["WRKLOG_TYPE_CODE"] = "Q"; // EOL
        dr["SHFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
        dr["EQPTID"] = "ALL";

        dtRqst.Rows.Add(dr);

        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_WRKLOG_ALL", "RQSTDT", "RSLTDT", dtRqst);

        if (dtRslt.Rows.Count == 0) return false; // 실적 미확정
        else
        {
            //실적 확정자 이름 가져오기
            DataTable dtRqst2 = new DataTable();
            dtRqst2.Columns.Add("LANGID", typeof(string));
            dtRqst2.Columns.Add("USERID", typeof(string));

            DataRow dr2 = dtRqst2.NewRow();
            dr2["LANGID"] = LoginInfo.LANGID;
            dr2["USERID"] = Util.NVC(dtRslt.Rows[0]["RSLT_CNFM_USERID"]);

            dtRqst2.Rows.Add(dr2);
            DataTable dtrslt2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_ID", "RQSTDT", "RSLTDT", dtRqst2);

            if (dtRslt.Rows.Count == 0) txtWorkUser.Text = Util.NVC(dtRslt.Rows[0]["RSLT_CNFM_USERID"]);
            else txtWorkUser.Text = Util.NVC(dtrslt2.Rows[0]["USERNAME"]);

            return true;
        }//실적 확정
    }*/

        private void SetGridPerf(C1DataGrid dg, DataTable dt, DataTable dtDetail)
        {
            try
            {
                dg.ClearFilter();

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
                    Util.GridSetData(dg, dt, this.FrameOperation, true);
                }

                string[] sColumnName = new string[] { "MDLLOT_ID", "EQSGID", "PROD_LOTID" };
                _Util.SetDataGridMergeExtensionCol(dg, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                // 구분 컬럼 콤보 설정
                if (dt.Columns.Contains("LOT_ATTR"))
                {
                    DataTable dtFlag = new DataTable("FLAG");
                    dtFlag.Columns.Add("CODE", typeof(string));
                    dtFlag.Columns.Add("NAME", typeof(string));

                    List<string> flagList = dt.AsEnumerable().Select(s => s.Field<string>("LOT_ATTR")).Distinct().ToList();
                    foreach (string flag in flagList)
                    {
                        DataRow drNew = dtFlag.NewRow();
                        drNew["CODE"] = flag;
                        drNew["NAME"] = flag;
                        dtFlag.Rows.Add(drNew);
                    }
                    dtFlag.AcceptChanges();

                    if (dtFlag.Rows.Count > 0) dg.SetGridColumnCombo("LOT_ATTR", dtFlag, "", false, false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
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
            FCS001_054_DFCT_CELL_LIST window = sender as FCS001_054_DFCT_CELL_LIST;
        }

        #region LotType MultSelection
        //2023.02.07 LotType MultSelection 
        private void SetLotTypeCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                dtRqstA.Rows.Add(drA);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_CMN_ALL_ITEMS", "RQSTDT", "RSLTDT", dtRqstA);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTTYPE_CBO", "RQSTDT", "RSLTDT", dtRqstA);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();

            //C20220603-000198_저전압 1차/2차 선택 기능 추가 START
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_110"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {
                rdoLowVolt.Visibility = Visibility.Visible;
                rdo2LowVolt.Visibility = Visibility.Visible;
            }
            else
            {
                rdoLowVolt.Visibility = Visibility.Collapsed;
                rdo2LowVolt.Visibility = Visibility.Collapsed;
            }
            //C20220603-000198_저전압 1차/2차 선택 기능 추가 END

            SetGridHeader(dgDIR);    //직행 작업일지 Header Setting
            SetGridHeader(dgRework); //재작업 작업일지 Header Setting

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgDirList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null)
            {
                if (datagrid.CurrentCell == null || datagrid.CurrentRow == null || datagrid.CurrentColumn == null) return;
                if (string.IsNullOrEmpty(Util.NVC(datagrid.CurrentColumn.Tag))) return;
                if (datagrid.CurrentRow.Type != DataGridRowType.Bottom) return;

                if (datagrid.CurrentRow.Type == DataGridRowType.Bottom && datagrid.FilteredColumns.Length > 0)
                {
                    //필터가 적용된 경우 합계 목록을 조회 할 수 없습니다.
                    Util.MessageConfirm("FM_ME_0479", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            datagrid.ClearFilter();
                        }
                    });
                    return;
                }

                cell = datagrid.CurrentCell;
            }
            else
            {
                if (cell.Row.Type != DataGridRowType.Item) return;
            }

            if (string.IsNullOrEmpty(Util.NVC(cell.Text)) || Util.NVC(cell.Text).Equals("0")) return;

            FCS001_054_DFCT_CELL_LIST wndPopup = new FCS001_054_DFCT_CELL_LIST();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                int row = cell.Row.Index;
                object[] Parameters = new object[14];


                if (datagrid.CurrentRow.Type == DataGridRowType.Bottom)
                {
                    for (int i = 2; i <= 7; i++)
                    {
                        Parameters[i] = "";
                    }
                    Parameters[3] = Util.GetCondition(cboEQSGID, bAllNull: true); //EQSGID
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
                Parameters[9] = ""; // RWK_FLAG
                Parameters[10] = ""; // ROUT_TYPE_CODE
                if (datagrid.Name.Equals("dgDIR"))
                {
                    Parameters[0] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                    Parameters[1] = Util.GetCondition(cboShift, bAllNull: true); //SHFT_ID
                    Parameters[8] = "AFTER_DEGAS_OCV_SELECTOR_FRST_MOVE";
                    if (rdo2LowVolt.IsChecked.Equals(true))
                    {
                        Parameters[8] = "AFTER_DEGAS_OCV2_SELECTOR_FRST_MOVE";
                    }
                    Parameters[11] = "";
                }
                else
                {
                    Parameters[0] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                    Parameters[1] = Util.GetCondition(cboShift, bAllNull: true); //SHFT_ID
                    Parameters[8] = "AFTER_DEGAS_OCV_SELECTOR_RWK_MOVE";
                    if (rdo2LowVolt.IsChecked.Equals(true))
                    {
                        Parameters[8] = "AFTER_DEGAS_OCV2_SELECTOR_RWK_MOVE";
                    }
                    Parameters[11] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PRE_DFCT_CODE")); //이전불량명
                }
                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }

        }

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

                if (!string.IsNullOrEmpty(tag))
                {
                    if (tag.Equals("A"))
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    else e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

                if (!string.IsNullOrEmpty(tag) && e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    if (dataGrid.FilteredColumns.Length == 0)
                    {
                        if (tag.Equals("A"))
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                        else
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = Brushes.Black;
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item || e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        //C20220603-000198_저전압 1차/2차 선택 기능 추가 START
        private void rdoLowVolt_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgDIR);
            Util.gridClear(dgRework);
        }
        //C20220603-000198_저전압 1차/2차 선택 기능 추가 END

        //C20220603-000198_저전압 1차/2차 선택 기능 추가 START
        private void rdoLowVolt_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgDIR);
            Util.gridClear(dgRework);
        }
        //C20220603-000198_저전압 1차/2차 선택 기능 추가 END


        private void cboLotType_Loaded(object sender, RoutedEventArgs e)
        {
            if (cboLotType.ItemsSource == null) SetLotTypeCombo(cboLotType); //LoTtype MultiComboBox 추가
        }

        private void cboLotType_SelectionChanged(object sender, EventArgs e)
        {
            if (cboLotType.SelectedItems.Count == 0)
            {
                cboLotType.CheckAll();
            }
        }

        #endregion
    }

}
