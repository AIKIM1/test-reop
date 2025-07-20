/*************************************************************************************
 Created Date : 2021.01.
      Creator : 
   Decription : 충방전 실적관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.01. DEVELOPER : Initial Created.
  2021.08.18  강동희 : 특이사항 입/출력 기능 추가
  2023.02.10  임근영 : 특이사항 저장시 라인정보 함께 저장하도록 추가 
  2023.03.27  조영대 : 불량명 컬럼헤더 다국어 처리 오류 수정, 필터 적용시 합계 수정
  2023.04.17  하유승 : MultiSelectionBox 추가
  2023.08.01  이정미 : 충방전 실적 관리 상세 팝업 > Parameters[3] 추가 
  2023.08.17  이정미 : Parameters 변수 배열 크기 수정 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_057 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private DataTable[] dtHeader = new DataTable[4];
        public FCS001_057()
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
            _combo.SetCombo(cboShift2, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_SHIFT");
            _combo.SetCombo(cboEQSGID, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");
            _combo.SetCombo(cboEQSGID2, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");

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
            return dtRslt;
        }
        private void GetListBF(string _L_flag)
        {
            try
            {
                Util.gridClear(dgFormBFD);
                Util.gridClear(dgFormBF);

                GetSummaryData("A", _L_flag);
                GetRemarkData("B"); //2021.08.18 특이사항 입/출력 기능 추가 //1페이지
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
        private void GetListAF(string _L_flag)
        {
            try
            {
                Util.gridClear(dgFormAFD);
                Util.gridClear(dgFormAF);
                GetSummaryData("B", _L_flag);
                GetRemarkData("A"); //2021.08.18 특이사항 입/출력 기능 추가//2페이지
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

        private void GetSummaryData(string WRKLOG_TYPE_CODE, string _L_flag)
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
                dtRqst.Columns.Add("LOTTYPE", typeof(string));

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
                
                dr["LOTTYPE"] = getMcbData(_L_flag.Equals("BF") ? cboLotType2 : cboLotType); //BF 1페이지
               
                dtRqst.Rows.Add(dr);
                DataSet InDataSet = new DataSet();
                InDataSet.Tables.Add(dtRqst);

                ShowLoadingIndicator();
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

                    HiddenLoadingIndicator();
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

                    HiddenLoadingIndicator();
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
                dtRqst.Columns.Add("EQSGID", typeof(string));//


                DataRow dr = dtRqst.NewRow();
                dr["WRKLOG_TYPE_CODE"] = WRKLOG_TYPE_CODE;
                if (WRKLOG_TYPE_CODE.Equals("A")) 
                {
                    dr["WRK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
                    dr["SHFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);  //
                    if (string.IsNullOrEmpty(Util.GetCondition(cboEQSGID2, bAllNull: true))) //CBOEQSGID2
                    {
                        dr["EQSGID"] = "ALL";
                    }//
                    else
                    {
                        dr["EQSGID"] = Util.GetCondition(cboEQSGID2, bAllNull: true); 
                    }

                }
                else                                     
                {
                    dr["WRK_DATE"] = dtpWorkDate2.SelectedDateTime.ToString("yyyyMMdd");
                    dr["SHFT_ID"] = Util.GetCondition(cboShift2, bAllNull: true);   //
                    if (string.IsNullOrEmpty(Util.GetCondition(cboEQSGID, bAllNull: true))) //CBOEQSGID
                    {
                        dr["EQSGID"] = "ALL";
                    }
                    else
                    {
                        dr["EQSGID"] = Util.GetCondition(cboEQSGID, bAllNull: true);
                    }
                }
                

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RSLT_REMARK", "RQSTDT", "RSLTDT", dtRqst);
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

        private object getMcbData(MultiSelectionBox mcb) => mcb.GetBindValue();
        #endregion
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
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
                }
                else if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    if (!string.IsNullOrEmpty(tag))
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //DEGAS 전 1차 충전
            dtHeader[0] = SetGridHeader(dgFormBFD, "A", null).DefaultView.ToTable(false, new string[] { "DFCT_CODE" }); //Degas 전 직행
            dtHeader[2] = SetGridHeader(dgFormBF, "A", "B").DefaultView.ToTable(false, new string[] { "DFCT_CODE" }); //Degas 전 재작업 일지 Header Setting

            //DEGAS 후 2차 충전
            dtHeader[1] = SetGridHeader(dgFormAFD, "B", null).DefaultView.ToTable(false, new string[] { "DFCT_CODE" }); //Degas 후 직행
            dtHeader[3] = SetGridHeader(dgFormAF, "B", "B").DefaultView.ToTable(false, new string[] { "DFCT_CODE" }); //Degas 후 재작업 일지 Header Setting

            InitControl();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetListAF("AF");//2페이지
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetListBF("BF");//1페이지
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

                    if(datagrid.Name.Equals("dgFormBFD"))
                    {
                        Parameters[3] = Util.GetCondition(cboEQSGID, bAllNull: true); //EQSGID
                    }
                    else
                    {
                        Parameters[3] = Util.GetCondition(cboEQSGID2, bAllNull: true); //EQSGID
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

        private void dgRwkList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                object[] Parameters = new object[12];

                if (datagrid.CurrentRow.Type == DataGridRowType.Bottom)
                {
                    for (int i = 2; i <= 7; i++)
                    {
                        Parameters[i] = "";
                    }
                    if (datagrid.Name.Equals("dgFormBF"))
                    {
                        Parameters[3] = Util.GetCondition(cboEQSGID, bAllNull: true); //EQSGID
                    }
                    else
                    {
                        Parameters[3] = Util.GetCondition(cboEQSGID2, bAllNull: true); //EQSGID
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

        //2021.08.18 특이사항 입/출력 기능 추가 START
        private void btnSaveB_Click(object sender, RoutedEventArgs e)  //1페이지
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
                        dtRqst.Columns.Add("EQSGID", typeof(string));//

                        DataRow dr = dtRqst.NewRow();
                        dr["WRK_DATE"] = dtpWorkDate2.SelectedDateTime.ToString("yyyyMMdd"); //DTPWORKDATE2
                        dr["SHFT_ID"] = Util.GetCondition(cboShift2);
                        dr["WRKLOG_TYPE_CODE"] = "B";
                        dr["REMARKS_CNTT"] = Util.GetCondition(txtRemarkB);
                        dr["USERID"] = LoginInfo.USERID;
                        if (string.IsNullOrEmpty(Util.GetCondition(cboEQSGID, bAllNull: true))) //CBOEQSGID 
                        {
                            dr["EQSGID"] = "ALL";
                        }
                        else
                        {
                            dr["EQSGID"] = Util.GetCondition(cboEQSGID, bAllNull: true); 
                        }

                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RSLT_REMARKS", "INDATA", "OUTDATA", dtRqst);

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
                        dtRqst.Columns.Add("EQSGID", typeof(string));//
                        

                        DataRow dr = dtRqst.NewRow();
                        dr["WRK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
                        dr["SHFT_ID"] = Util.GetCondition(cboShift);
                        dr["WRKLOG_TYPE_CODE"] = "A";  
                        dr["REMARKS_CNTT"] = Util.GetCondition(txtRemarkA);
                        dr["USERID"] = LoginInfo.USERID;
                        if (string.IsNullOrEmpty(Util.GetCondition(cboEQSGID2, bAllNull: true))) //cboEQSGID2
                        {
                            dr["EQSGID"] = "ALL";
                        }
                        else
                        {
                            dr["EQSGID"] = Util.GetCondition(cboEQSGID2, bAllNull: true);  
                        }

                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RSLT_REMARKS", "INDATA", "OUTDATA", dtRqst);

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

        private void cboLotType_Loaded(object sender, RoutedEventArgs e)
        {
            if (_tabControl.SelectedIndex == 0) return;
            if (cboLotType.ItemsSource == null) SetLotTypeCombo(cboLotType); //LoTtype MultiComboBox 추가
        }

        private void cboLotType2_Loaded(object sender, RoutedEventArgs e)
        {
            if (_tabControl.SelectedIndex == 1) return;
            if (cboLotType2.ItemsSource == null) SetLotTypeCombo(cboLotType2); //LoTtype MultiComboBox 추가
        }

        private void cboLotType_SelectionChanged(object sender, EventArgs e)
        {
            if (sender is MultiSelectionBox)
            {
                McbObjCheckCount(_tabControl.SelectedIndex == 0 ? cboLotType2 : cboLotType); // tab index 0 = page 1
            }
        }

        private void McbObjCheckCount(MultiSelectionBox mcb)
        {
            if(mcb.SelectedItems.Count == 0)
            {
                mcb.CheckAll();
            }
        }

        //2021.08.18 특이사항 입/출력 기능 추가 END
        #endregion
    }
}
