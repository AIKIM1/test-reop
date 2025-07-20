/*************************************************************************************
 Created Date : 2021.04.11
      Creator : 
   Decription : 저전압, 용량배출 실적관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.01. DEVELOPER : Initial Created.
  2022.07.25  KDH    : C20220603-000198_저전압 1차/2차 선택 기능 추가
 
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

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_110 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtHeader = new DataTable();

        bool bUseFlag = false;

        public FCS002_110()
        {
            InitializeComponent();

            InitCombo();
            InitControl();

            //C20220603-000198_저전압 1차/2차 선택 기능 추가 START
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS002_110"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
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

            //
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
            dtHeader = dtRslt.DefaultView.ToTable(false, new string[] { "DFCT_CODE" });
        }

        private void GetList()
        {
            try
            {
                Util.gridClear(dgDIR);
                Util.gridClear(dgRework);
                GetSummaryData();
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

        private void GetSummaryData()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WORK_DATE", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
                dr["WORK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd");

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
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_FORMATION_PERF", "INDATA", "OUT_DIR,OUT_DIR_DETAIL,OUT_RWK,OUT_RWK_DETAIL", InDataSet);

                DataTable dtDir = dsRslt.Tables["OUT_DIR"];
                DataTable dtDirDetail = dsRslt.Tables["OUT_DIR_DETAIL"];
                DataTable dtRwk = dsRslt.Tables["OUT_RWK"];
                DataTable dtRwkDetail = dsRslt.Tables["OUT_RWK_DETAIL"];

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
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
                if (e.Cell.Row.Index == dataGrid.Rows.Count() - 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
            }));
        }


        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_054_DFCT_CELL_LIST window = sender as FCS002_054_DFCT_CELL_LIST;
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

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
                    Parameters[9] = ""; // RWK_FLAG
                    Parameters[10] = ""; // ROUT_TYPE_CODE
                    if (datagrid.Name.Equals("dgDIR"))
                    {
                        Parameters[0] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                        Parameters[1] = Util.GetCondition(cboShift, bAllNull: true); //SHFT_ID
                        Parameters[8] = "AFTER_DEGAS_OCV_SELECTOR_FRST_MOVE";
                        Parameters[11] = "";
                    }
                    else
                    {
                        Parameters[0] = dtpWorkDate.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                        Parameters[1] = Util.GetCondition(cboShift, bAllNull: true); //SHFT_ID
                        Parameters[8] = "AFTER_DEGAS_OCV_SELECTOR_RWK_MOVE";
                        Parameters[11] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "LOT_COMENT"));
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
        #endregion

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

    }

}
