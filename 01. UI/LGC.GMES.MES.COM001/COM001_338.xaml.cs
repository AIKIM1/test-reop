/*************************************************************************************
 Created Date : 2020.11.02
      Creator : 안인효
   Decription : 공정 NG 정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.02  안인효 : Initial Created.   
 
**************************************************************************************/

using C1.WPF;
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
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using System.Linq;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_338 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        private const string _assembly = "ASSY";

        DataRow _drSelectRow;

        public COM001_338()
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

        #region # Combo Setting
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESSWITHAREA");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            // 극성
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
        }
        #endregion

        #region # Control Clear
        private void InitUsrControl()
        {
            _drSelectRow = null;
        }

        private void InitGridControl()
        {
            Util.gridClear(dgLotList);
        }

        private void SetControl()
        {
            //dgLotList.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            //dgLotList.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(35);

            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;

            //dgDefect.AlternatingRowBackground = null;
            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
        }

        #endregion

        #endregion

        #region Event

        #region # Form Load
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            InitCombo();
            //InitColumnsList();
            SetControl();
            //SetAreaGridVisibility();
            //SetSearchControl();

            GetCaldate();
            //SetControlVisibility(cboProcess.SelectedValue.ToString());

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region # 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetLotList();
        }
        #endregion

        #region # 조회조건 : 동 변경
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.Items.Count > 0 && cboArea.SelectedValue != null && !cboArea.SelectedValue.Equals("SELECT"))
            {
                //SetAreaGridVisibility();
            }
        }
        #endregion

        #region # 조회조건 : 라인 변경
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            //{
            //    SetProcess();
            //}
        }
        #endregion

        #region # 조회조건 : 공정 변경
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                //SetEquipment();
                InitUsrControl();
                InitGridControl();
                //SetAreaGridVisibility();
                //SetSearchControl();

                //SetControlVisibility(cboProcess.SelectedValue.ToString());

                //SetCoatingLineColumnVisibility();
            }
            else
            {
                //dgLotList.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["COATING_LINE_NOTCHING"].Visibility = Visibility.Collapsed;
                //dgInputHist.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region # 조회조건 : 작업일 변경
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region # 조회조건 : Lot KeyDown
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region # 조회조건 : 프로젝트 KeyDown
        private void txtPrjtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        private void dgLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            popupAssyCell(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "LOTID")),
                                        Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "AREAID")),
                                        Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "EQSGID")),
                                        Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PROCID")),
                                        Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "EQPTID")),
                                        Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PRJT_NAME")),
                                        Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PRDT_CLSS_CODE")),
                                        dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"),
                                        dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if (e.Cell.Column.Name.Equals("LOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
            }));
        }


        #endregion

        #region Mehod

        #region # 실적 일자 조회
        public void GetCaldate()
        {
            try
            {
                ShowLoadingIndicator();

                dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-30);

                //DataTable dtRqst = new DataTable();
                //dtRqst.Columns.Add("AREAID", typeof(string));
                //dtRqst.Columns.Add("DTTM", typeof(string));

                //DataRow dr = dtRqst.NewRow();
                //dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //dtRqst.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                //if (dtRslt != null && dtRslt.Rows.Count > 0)
                //{
                //    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                //    dtpDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region # 공정 콤보
        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 설비 콤보
        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # NG Lot 리스트 조회
        /// <summary>
        /// 생산 Lot 리스트 조회
        /// </summary>
        public void GetLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                dr["EQPTID"] = string.IsNullOrWhiteSpace(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue.ToString();
                //dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                //dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                dr["PRDT_CLSS_CODE"] = cboElecType.SelectedValue.ToString() == "" ? null : cboElecType.SelectedValue.ToString();

                if (!string.IsNullOrWhiteSpace(txtLotId.Text))
                    dr["LOTID"] = txtLotId.Text;
                else if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                dtRqst.Rows.Add(dr);

                string bizRuleName = string.Empty;

                bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_DFCT_DATA_CLCT_GROUP";
                //InitUsrControl();

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgLotList, bizResult, FrameOperation, true);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion


        private void popupAssyCell(string sLotID, string strAreaID, string strEqsgID, string strProcID, string strEqptID, string strPrjtName, string strPrdtClssCode, string strFromDate, string strToDate)
        {
            COM001_337 popAssyCell = new COM001_337();
            popAssyCell.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popAssyCell.Name.ToString()) == false)
                return;

            object[] Parameters = new object[9];
            //Parameters[0] = Util.NVC(_drSelectRow["LOTID"]);
            Parameters[0] = sLotID;
            Parameters[1] = strAreaID;
            Parameters[2] = strEqsgID;
            Parameters[3] = strProcID;
            Parameters[4] = strEqptID;
            Parameters[5] = strPrjtName;
            Parameters[6] = strPrdtClssCode;
            Parameters[7] = strFromDate;
            Parameters[8] = strToDate;

            C1WindowExtension.SetParameters(popAssyCell, Parameters);

            popAssyCell.Closed += new EventHandler(popAssyCell_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popAssyCell.ShowModal()));
        }

        private void popAssyCell_Closed(object sender, EventArgs e)
        {
            COM001_337 popup = sender as COM001_337;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }


        #endregion

        #region Function

        #region [Validation]
        private bool ValidationSearch(bool isLot = false)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (!isLot)
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 동을선택하세요
                    Util.MessageValidation("SFU1499");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationPopupQuality()
        {
            if (string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            return true;
        }

        #endregion

        #region [Popup]



        #endregion

        #region [Func]
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        #region # 그리드, 탭, Text Header Text 및 칼럼 Visibility

        private void SetGridVisibility(DataTable dt, C1DataGrid dg, string ProcID)
        {
            dg.UpdateLayout();

            foreach (DataRow dr in dt.Rows)
            {
                string[] sProcess = dr["PROCESS"].ToString().Split(',');
                string[] sExceptionProcess = dr["EXCEPTION_PROCESS"].ToString().Split(',');
                int nExceptionindex;
                int nindex = -1;

                // 예외 공정 검색 
                nExceptionindex = Array.IndexOf(sExceptionProcess, ProcID);
                if (nExceptionindex >= 0)
                {
                    if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // 대상공정 여부 체크
                    nindex = Array.IndexOf(sProcess, ProcID);

                    // 검색 공정이 없으면 조립 구분자로 다시 검색
                    if (nindex < 0)
                    {
                        nindex = Array.IndexOf(sProcess, _assembly);
                    }

                    // 공정별 칼럼 Visibility
                    if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        if (nindex < 0)
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Collapsed;
                        else
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void SetGridFormat(C1DataGrid dg, string ProcID)
        {
            string sFormat = string.Empty;

            if (ProcID.Equals(Process.NOTCHING))
            {
                sFormat = "###,##0.##";
            }
            else 
            {
                sFormat = "###,##0";
            }

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["PRE_PROC_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["FIX_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["RMN_QTY"])).Format = sFormat;
        }

        private string SetGridFormatBottomRow(string ColumnName, object obj)
        {
            string sFormat = string.Empty;
            double dFormat = 0;

            if (cboProcess.SelectedValue != null && cboProcess.SelectedValue.ToString() == Process.NOTCHING)
            {
                if (ColumnName == "PRE_PROC_LOSS_QTY" || ColumnName == "FIX_LOSS_QTY" || ColumnName == "RMN_QTY")
                    sFormat = "{0:###,##0.##}";
                else
                    sFormat = "{0:###,##0}";
            }
            else
            {
                sFormat = "{0:###,##0}";
            }

            if (Double.TryParse(Util.NVC(obj), out dFormat))
                return String.Format(sFormat, dFormat);

            return string.Empty;
        }

        #endregion

        #endregion

        #endregion
    }
}