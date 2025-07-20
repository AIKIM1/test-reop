/*************************************************************************************
 Created Date : 2023.07.03
      Creator : 
   Decription : 포장 PALLET 재고실사
--------------------------------------------------------------------------------------
 [Change History]
  2023.07.03  백광영 : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어
  2024.05.24  윤지해 : (수정사항 없음) NERP 대응 프로젝트-차수마감 취소 등 개발 범위에서 제외(대상테이블 아님)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System.Configuration;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_387 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        CommonCombo _combo = new CommonCombo();

        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;

        private string _dicNameTotal = ObjectDic.Instance.GetObjectName("Total");

        private string _keySTCK_CNT_YM = string.Empty;
        private string _keyRACK_ID = string.Empty;

        private bool _sFirstLoading = true;
        private bool _sInventoryCheckResultYN = true;



        public COM001_387()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        DataView _dvSTCKCNT { get; set; }

        string _sSTCK_CNT_CMPL_FLAG = string.Empty;

        #endregion

        #region Initialize

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preCellHold = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        CheckBox chkAllCellHold = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            ldpMonthShot.SelectedDateTime = System.DateTime.Now;
            ldpMonthResult.SelectedDateTime = System.DateTime.Now;

            CommonCombo _combo = new CommonCombo();
            #region Tab1
            // Area
            C1ComboBox[] cboAreaChild = { cboSection };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild);

            //// Location
            //object[] objbldgParent = { cboArea };
            //String[] sFilterAll1 = { "" };
            //_combo.SetComboObjParent(cboBldg, CommonCombo.ComboStatus.NONE, sCase: "AREA_BLDG", objParent: objbldgParent, sFilter: sFilterAll1);

            // Section
            object[] objSectionParent = { cboArea };
            C1ComboBox[] cboInventorySeqChild = { cboInventorySeq };
            String[] sFilterAll2 = { "" };
            _combo.SetComboObjParent(cboSection, CommonCombo.ComboStatus.NONE, sCase: "AREA_SECTION", objParent: objSectionParent, cbChild: cboInventorySeqChild, sFilter: sFilterAll2);

            // 차수
            object[] objStockSeqShotParent = { ldpMonthShot, cboArea, cboSection };
            String[] sFilterAll = { "" };
            _combo.SetComboObjParent(cboInventorySeq, CommonCombo.ComboStatus.NONE, sCase: "SECTION_STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterAll);
            #endregion

            #region Tab2
            // Area
            C1ComboBox[] cboAreaResultChild = { cboSectionResult };
            _combo.SetCombo(cboAreaResult, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaResultChild);

            // Section
            object[] objSectionResultParent = { cboAreaResult };
            C1ComboBox[] cboInventoryResultSeqChild = { cboInventoryResultSeqNo };
            String[] sFilterTabAll2 = { "" };
            _combo.SetComboObjParent(cboSectionResult, CommonCombo.ComboStatus.NONE, sCase: "AREA_SECTION", objParent: objSectionResultParent, cbChild: cboInventoryResultSeqChild, sFilter: sFilterTabAll2);

            // 차수
            object[] objInventoryStockSeqShotParent = { ldpMonthResult, cboAreaResult, cboSectionResult };
            String[] sFilterTabAll = { "" };
            _combo.SetComboObjParent(cboInventoryResultSeqNo, CommonCombo.ComboStatus.NONE, sCase: "SECTION_STOCKSEQ", objParent: objInventoryStockSeqShotParent, sFilter: sFilterTabAll);

            #endregion

        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            _scrollToHorizontalOffset = dgListShot.Viewport.HorizontalOffset;

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnExclude_SNAP);
            //listAuth.Add(btnExclude_RSLT);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        // 차수추가
        private void btnAddSeq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_387_ADD_SEQ wndSTOCKCNT_ADD = new COM001_387_ADD_SEQ();
                wndSTOCKCNT_ADD.FrameOperation = FrameOperation;

                if (wndSTOCKCNT_ADD != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = Convert.ToString(cboArea.SelectedValue);
                    Parameters[1] = ldpMonthShot.SelectedDateTime;
                    Parameters[2] = Convert.ToString(cboSection.SelectedValue);

                    C1WindowExtension.SetParameters(wndSTOCKCNT_ADD, Parameters);

                    wndSTOCKCNT_ADD.Closed += new EventHandler(wndSTOCKCNT_ADD_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_ADD.ShowModal()));
                    wndSTOCKCNT_ADD.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndSTOCKCNT_ADD_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_387_ADD_SEQ window = sender as COM001_387_ADD_SEQ;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    CommonCombo _combo = new CommonCombo();
                    _combo.SetCombo(cboInventorySeq);
                    _combo.SetCombo(cboInventoryResultSeqNo);

                    Util.gridClear(dgListShot);
                    Util.gridClear(dgListDetail);
                    Util.gridClear(dgListLocationSummary);
                    Util.gridClear(dgcheckResult);

                    SetListShot();

                    _keySTCK_CNT_YM = string.Empty;    // 선택 Key Clear
                    _keyRACK_ID = string.Empty;
                    SetListResult();  // POPUP STOCKCNT CLOSE
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 재고 조회
        private void btnSearchShot_Click(object sender, RoutedEventArgs e)
        {
            if (_sFirstLoading)
                _sFirstLoading = false;

           
            string sArea = Util.GetCondition(cboArea);
            if (string.IsNullOrWhiteSpace(sArea))
            {
                Util.MessageValidation("SFU1499");   // 동을 선택하세요.
                return;
            }

            
            string sSection = Util.GetCondition(cboSection);
            if (string.IsNullOrWhiteSpace(sSection))
            {
                Util.MessageValidation("SFU8912");  // 창고를 선택하세요.
                return;
            }

            
            string sInventorySeq = Util.GetCondition(cboInventorySeq);
            if (string.IsNullOrWhiteSpace(sInventorySeq))
            {
                Util.MessageValidation("SFU2958");  // 차수는 필수 입니다.
                return;
            }

            Util.gridClear(dgListShot);
            Util.gridClear(dgListDetail);

            SetListShot();

        }

        // 재고조사 조회
        private void btnSearchResult_Click(object sender, RoutedEventArgs e)
        {
            if (_sFirstLoading)
                _sFirstLoading = false;

            
            string sArea = Util.GetCondition(cboAreaResult);
            if (string.IsNullOrWhiteSpace(sArea))
            {
                Util.MessageValidation("SFU1499");  // 동을 선택하세요.
                return;
            }

            
            string sSection = Util.GetCondition(cboSectionResult);
            if (string.IsNullOrWhiteSpace(sSection))
            {
                Util.MessageValidation("SFU8912");  // 창고를 선택하세요.
                return;
            }

            // 차수는 필수 입니다.
            string sInventorySeq = Util.GetCondition(cboInventoryResultSeqNo);
            if (string.IsNullOrWhiteSpace(sInventorySeq))
            {
                Util.MessageValidation("SFU2958");
                return;
            }

            Util.gridClear(dgListLocationSummary);
            Util.gridClear(dgcheckResult);

            // 선택 Row Clear
            _keyRACK_ID = string.Empty;  
            _keySTCK_CNT_YM = string.Empty;

            SetListResult();  // Button SearchResult Click  

        }

        private void ldpMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            _combo.SetCombo(cboInventorySeq);

            Util.gridClear(dgListShot);
            Util.gridClear(dgListDetail);
        }

        private void ldpMonthResult_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            _combo.SetCombo(cboInventoryResultSeqNo);

            Util.gridClear(dgListLocationSummary);
            Util.gridClear(dgcheckResult);
        }

        private void cboSection_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void chkDiffFlag_Checked(object sender, RoutedEventArgs e)
        {
            if (_sFirstLoading) return;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgListLocationSummary, "CHK");
            Util.gridClear(dgcheckResult);

            if (idx < 0)
            {
                _keySTCK_CNT_YM = string.Empty;
                _keyRACK_ID = string.Empty;
            }
            else
            {
                DataRowView drv = dgListLocationSummary.Rows[idx].DataItem as DataRowView;
                _keySTCK_CNT_YM = Convert.ToString(drv["STCK_CNT_YM"]);
                _keyRACK_ID = Convert.ToString(drv["RACK_ID"]);
            }

            SetListResult();  // Diff Flag Checked

            idx = _Util.GetDataGridCheckFirstRowIndex(dgListLocationSummary, "CHK");
            if (idx > -1 && _sInventoryCheckResultYN)
            {
                GetInventoryCheckResult(idx , string.Empty);   // Diff Flag Checked
            }
        }

        private void chkDiffFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_sFirstLoading) return;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgListLocationSummary, "CHK");
            Util.gridClear(dgcheckResult);

            if (idx < 0)
            {
                _keySTCK_CNT_YM = string.Empty;
                _keyRACK_ID = string.Empty;
            }
            else
            {
                DataRowView drv = dgListLocationSummary.Rows[idx].DataItem as DataRowView;
                _keySTCK_CNT_YM = Convert.ToString(drv["STCK_CNT_YM"]);
                _keyRACK_ID = Convert.ToString(drv["RACK_ID"]);
            }

            SetListResult();   // Diff Flag UnChecked

            idx = _Util.GetDataGridCheckFirstRowIndex(dgListLocationSummary, "CHK");
            if (idx > -1 && _sInventoryCheckResultYN)
            {
                GetInventoryCheckResult(idx , string.Empty);   // Diff Flag UnChecked
            }
        }

        private void dgListLocationChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb == null)
                return;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                int rowIndex = _Util.GetDataGridCheckFirstRowIndex(dgListLocationSummary, "CHK");
                if (rowIndex > -1)
                {
                    GetInventoryCheckResult(rowIndex , string.Empty);   // dgListLocationSummary Grid Location Change
                }
            }
        }

        // Inventory Check Result
        private void GetInventoryCheckResult(int idx , string _type)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                C1DataGrid dg = dgListLocationSummary;

                string sSTCK_CNT_YM = Util.GetCondition(ldpMonthShot);
                string sAREAID = Util.GetCondition(cboArea);
                string sWH_ID = Util.GetCondition(cboSection);
                string sSTCK_CNT_SEQNO = Util.GetCondition(cboInventorySeq);
                string sRACK_ID = null;
                string sBizRule = chkDiffFlag.IsChecked == true ? "DA_PRD_GET_WH_STCK_CNT_RSLT_CFM_DIFF" : "DA_PRD_GET_WH_STCK_CNT_RSLT_CFM";

                if (!_type.Equals("Bottom"))
                {
                    sSTCK_CNT_YM = Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "STCK_CNT_YM"));
                    sAREAID = Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "AREAID"));
                    sWH_ID = Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "WH_ID"));
                    sSTCK_CNT_SEQNO = Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "STCK_CNT_SEQNO"));
                    sRACK_ID = Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "RACK_ID"));
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
                RQSTDT.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = sSTCK_CNT_YM;
                dr["AREAID"] = sAREAID;
                dr["WH_ID"] = sWH_ID;
                dr["STCK_CNT_SEQNO"] = sSTCK_CNT_SEQNO;
                dr["RACK_ID"] = Util.NVC(string.Empty).Equals(string.Empty) ? null : string.Empty;
                if (!_type.Equals("Bottom")) dr["RACK_ID"] = sRACK_ID;   // 합계부분 Rack ID *Blank

                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;
                if (dr["AREAID"].Equals("")) return;
                
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizRule, "RQSTDT", "RSLTDT", RQSTDT);
                DataView view = dtRslt.DefaultView;
                // 차이수량 체크 시 결과값 'OK','Location Inbound', 'Location OutBound' 제외
                if (chkDiffFlag.IsChecked == true)
                    view.RowFilter = "STCK_CNT_RSLT <> 'OK' AND STCK_CNT_RSLT <> 'Location Inbound' AND STCK_CNT_RSLT <> 'Location Outbound'";
                DataTable dt = view.ToTable();

                Util.GridSetData(dgcheckResult, dt, FrameOperation, true);
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

        // 차수마감
        private void btnClosing_Click(object sender, RoutedEventArgs e)
        {
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            
            string sArea = Util.GetCondition(cboAreaResult);
            if (string.IsNullOrWhiteSpace(sArea))
            {
                Util.MessageValidation("SFU1499");  // 동을 선택하세요.
                return;
            }

            
            string sSection = Util.GetCondition(cboSectionResult);
            if (string.IsNullOrWhiteSpace(sSection))
            {
                Util.MessageValidation("SFU8912");  // 창고를 선택하세요.
                return;
            }

            //마감하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1276"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DegreeClose();
                    Util.gridClear(dgListShot);
                    Util.gridClear(dgListDetail);

                    Util.gridClear(dgListLocationSummary);
                    Util.gridClear(dgcheckResult);
                }
            }
            );
        }

        // Confirm
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            List<int> list = _Util.GetDataGridCheckRowIndex(dgcheckResult, "CHK");
            if (list.Count <= 0)
            {
                
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return;
            }

            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU8565"); //선택된 재고실사 차수는 마감 상태입니다. 결과 반영 불가 합니다.
                return;
            }

            try
            {
                //실사 기준으로 반영하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8911"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        Confirm();
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboInventoryResultSeqNo_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _dvSTCKCNT = cboInventoryResultSeqNo.ItemsSource as DataView;

            string sStckCntSeq = cboInventoryResultSeqNo.Text;
            if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
            {
                _dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
                _sSTCK_CNT_CMPL_FLAG = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_CMPL_FLAG"].ToString();

                _dvSTCKCNT.RowFilter = null;
            }

        }
       
        private void dgListShot_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
                if ( colName != "STACK_QTY" )     return;

                if (dg.CurrentRow != null)
                {

                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;

                    if (dg.CurrentCell.Row.Type.ToString().Equals("Bottom"))
                    {
                        DataTable dt = ((DataView)dg.ItemsSource).ToTable().Clone();
                        DataView view = dt.AsDataView();

                        drvRow = view.AddNew();
                        drvRow["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                        drvRow["AREAID"] = Util.GetCondition(cboArea);
                        drvRow["WH_ID"] = Util.GetCondition(cboSection);
                        drvRow["STCK_CNT_SEQNO"] = Util.GetCondition(cboInventorySeq);
                        drvRow["RACK_ID"] = null;
                        drvRow.EndEdit();
                    }
                    ShowInventoryDetail(drvRow);

                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
            finally
            {
            }
        }
        // Inventory List
        private void ShowInventoryDetail(DataRowView drvRow)
        {
            ShowLoadingIndicator();
            DoEvents();

            Util.gridClear(dgListDetail);

            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("STCK_CNT_YM", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = drvRow["STCK_CNT_YM"].ToString();
                dr["AREAID"] = drvRow["AREAID"].ToString();
                dr["WH_ID"] = Util.NVC(drvRow["WH_ID"]);
                dr["STCK_CNT_SEQNO"] = Util.NVC(drvRow["STCK_CNT_SEQNO"]);
                dr["RACK_ID"] = Util.NVC(drvRow["RACK_ID"]).Equals(string.Empty) ? null : Util.NVC(drvRow["RACK_ID"]);
                INDATA.Rows.Add(dr);

                string _sbizName = "DA_PRD_GET_WH_STCK_CNT_SNAP_DTL";
                new ClientProxy().ExecuteService(_sbizName, "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Util.GridSetData(dgListDetail, result, FrameOperation, true);
                });
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

        private void dgListShot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                dg.Dispatcher.BeginInvoke(new Action(() =>
                {
                    string _col = e.Cell.Column.Name.ToString();
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (_col == "STACK_QTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Regular;
                    }
                }));

                if (_isscrollToHorizontalOffset)
                {
                    dg.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListLocationSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                dg.Dispatcher.BeginInvoke(new Action(() =>
                {
                    string _col = e.Cell.Column.Name.ToString();
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "GAB_LOT_QTY")).Equals("0"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                    if (e.Cell.Row.Type.ToString().Equals("Bottom"))
                    {
                        if (_col == "SNAP_LOT_QTY" || _col == "RSLT_LOT_QTY" || _col == "GAB_LOT_QTY")
                        {
                           // e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                          //  e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                        }
                    }
                }));

                if (_isscrollToHorizontalOffset)
                {
                    dg.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgcheckResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                string _col = e.Cell.Column.Name.ToString();
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string _rslt = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STCK_CNT_RSLT"));
                    if (!_rslt.Equals("OK") && !_rslt.Equals("Location Outbound") && !_rslt.Equals("Location Inbound"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    }
                }

            }));
        }

/*
        private void dgcheckResult_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null)
            {
                if (e.Column.Index == grid.Columns["CHK"].Index 
                    && !DataTableConverter.GetValue(e.Row.DataItem, "STCK_CNT_RSLT").Equals("NG")
                    && !DataTableConverter.GetValue(e.Row.DataItem, "STCK_CNT_RSLT").Equals("Diff-Loc"))
                {
                    e.Cancel = true;
                }
            }
        }
*/
        // 차수마감
        private void DegreeClose()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthResult);
                dr["AREAID"] = Util.GetCondition(cboAreaResult); 
                dr["WH_ID"] = Util.GetCondition(cboSectionResult); 
                dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboInventoryResultSeqNo));
                dr["USERID"] = LoginInfo.USERID;

                if (dr["STCK_CNT_YM"].Equals("")) return;
                if (dr["AREAID"].Equals("")) return;
                if (dr["WH_ID"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_WH_STCK_CNT_ORD_CMPL", "INDATA", null, RQSTDT);

                _combo.SetCombo(cboInventorySeq);
                _combo.SetCombo(cboInventoryResultSeqNo);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Confirm
        private void Confirm()
        {
            DataSet inData = new DataSet();

            // Indata
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("STCK_CNT_YM", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("WH_ID", typeof(string));
            inDataTable.Columns.Add("STCK_CNT_SEQNO", typeof(string));
            inDataTable.Columns.Add("RACK_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            DataRow row = null;

            row = inDataTable.NewRow();
            string _InventoryYM = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[0].DataItem, "STCK_CNT_YM")); 
            string _Area = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[0].DataItem, "AREAID"));
            string _WH = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[0].DataItem, "WH_ID"));
            string _SeqNo = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[0].DataItem, "STCK_CNT_SEQNO"));
            string _Rack = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[0].DataItem, "RACK_ID"));

            row["STCK_CNT_YM"] = _InventoryYM;
            row["AREAID"] = _Area;
            row["WH_ID"] = _WH;
            row["STCK_CNT_SEQNO"] = _SeqNo;
            row["RACK_ID"] = _Rack;
            row["USERID"] = LoginInfo.USERID;
            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            row["LANGID"] = LoginInfo.LANGID;

            inDataTable.Rows.Add(row);

            // Inventory Check Result
            DataTable inBox = inData.Tables.Add("INDATA_BOXID");
            inBox.Columns.Add("SNP_BOXID", typeof(string));
            inBox.Columns.Add("SNP_PLLT_BCD_ID", typeof(string));
            inBox.Columns.Add("RLT_BOXID", typeof(string));
            inBox.Columns.Add("RLT_PLLT_BCD_ID", typeof(string));

            if (dgcheckResult.Rows.Count > 0)
            {
                for (int i = 0; i < dgcheckResult.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inBox.NewRow();
                        row["SNP_BOXID"] = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[i].DataItem, "SNP_BOXID"));
                        row["SNP_PLLT_BCD_ID"] = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[i].DataItem, "SNP_PLLT_BCD_ID"));
                        row["RLT_BOXID"] = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[i].DataItem, "RLT_BOXID"));
                        row["RLT_PLLT_BCD_ID"] = Util.NVC(DataTableConverter.GetValue(dgcheckResult.Rows[i].DataItem, "RLT_PLLT_BCD_ID"));

                        inBox.Rows.Add(row);
                    }
                }
            }

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_WH_STCK_CNT_RSLT_CFM", "INDATA,INDATA_BOXID", null, inData);

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Util.gridClear(dgListLocationSummary);
                        Util.gridClear(dgcheckResult);

                        // 선택 Row Clear
                        _keySTCK_CNT_YM = string.Empty;
                        _keyRACK_ID = string.Empty;

                        SetListResult();  // 실사반영 이후

                        _Util.SetDataGridCheck(dgListLocationSummary, "CHK", "RACK_ID", _Rack);
                    }
                });
                return;

            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_WH_STCK_CNT_RSLT_CFM", ex.Message, ex.ToString());

            }
        }

        // Inventory Shapshot Summary
        private void SetListShot()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["WH_ID"] = Util.GetCondition(cboSection);
                dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboInventorySeq));

                if (dr["STCK_CNT_YM"].ToString().Equals("")) return;
                if (dr["AREAID"].Equals("")) return;
                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;
                if (dr["WH_ID"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_WH_STCK_CNT_SNAP_SUM", "RQSTDT", "RSLTDT", RQSTDT);

                if (result != null && result.Rows.Count > 0)
                {
                    Util.GridSetData(dgListShot, result, FrameOperation, true);

                    DataGridAggregate.SetAggregateFunctions(dgListShot.Columns["CONVERT_YM"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total")} }); 
                }
                else
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }
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

        // Location Summary
        private void SetListResult()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthResult);
                dr["AREAID"] = Util.GetCondition(cboAreaResult); 
                dr["WH_ID"] = Util.GetCondition(cboSectionResult);
                dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboInventoryResultSeqNo));

                if (dr["STCK_CNT_YM"].ToString().Equals("")) return;
                if (dr["AREAID"].Equals("")) return;
                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;
                if (dr["WH_ID"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_WH_STCK_CNT_SUM", "RQSTDT", "RSLTDT", RQSTDT);

                _sInventoryCheckResultYN = false;   // 이전 위치결과조회 OFF

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                    return;
                }

                DataView view = dtRslt.DefaultView;
                // 차이수량체크 시 수량 > 0 조회
                if (chkDiffFlag.IsChecked == true)
                    view.RowFilter = "GAB_LOT_QTY > 0";
                DataTable dt = view.ToTable();

                Util.GridSetData(dgListLocationSummary, dt, FrameOperation, true);
                // Total 추가
                DataGridAggregate.SetAggregateFunctions(dgListLocationSummary.Columns["CONVERT_YM"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });

                // 이전 위치 있으면 이전 위치로
                if (!string.IsNullOrEmpty(_keyRACK_ID))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow drSum = dt.Rows[i]; 
                        if (_keySTCK_CNT_YM.Equals(Convert.ToString(drSum["STCK_CNT_YM"]))
                            && _keyRACK_ID.Equals(Convert.ToString(drSum["RACK_ID"])))
                        {
                            DataTableConverter.SetValue(dgListLocationSummary.Rows[i].DataItem, "CHK", true);
                            dgListLocationSummary.SelectedIndex = i;
                            _sInventoryCheckResultYN = true; // 이전 위치있을경우 결과조회 실시
                            break;
                        }
                    }
                }
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

        private void chkResult_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
            object objRowIdx = dgcheckResult.Rows[idx].DataItem;

            if (objRowIdx != null)
            {
                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    DataTableConverter.SetValue(dgcheckResult.Rows[idx].DataItem, "CHK", true);
                }
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
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

        private int GetGridRowCount(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            return dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count;
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgcheckResult;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgcheckResult;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
        }

        private void dgListLocationSummary_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
                if (
                       colName != "SNAP_LOT_QTY"
                    && colName != "RSLT_LOT_QTY"
                    && colName != "GAB_LOT_QTY"
                    )
                {
                    return;
                }

                if (dg.CurrentRow != null)
                {
                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));
                    int rowIndex = dg.CurrentRow.Index;
                    string rowType = Convert.ToString(dg.CurrentCell.Row.Type);

                    GetInventoryCheckResult(rowIndex, rowType);  // Mouse Double Click 

                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void ckResultRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            DataTable dt = DataTableConverter.Convert(dgcheckResult.ItemsSource);
            DataRow row = dt.Rows[index];

            string _rslt = Util.NVC(row["STCK_CNT_RSLT"]);
            if (!_rslt.Equals("NG") && !_rslt.Equals("Diff-Loc"))
            {
                (dgcheckResult.GetCell(index, 0).Presenter.Content as CheckBox).IsChecked = false;
            }
        }

        private void dgcheckResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));

        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgcheckResult.ItemsSource == null) return;

            DataTable dt = ((DataView)dgcheckResult.ItemsSource).Table;

            foreach (DataRow dr in dt.Rows)
            {
                string _rslt = Util.NVC(dr["STCK_CNT_RSLT"]);
                if (_rslt.Equals("NG") || _rslt.Equals("Diff-Loc"))
                {
                    if (!Convert.ToBoolean(dr["CHK"])) dr["CHK"] = true;
                }
            }
            dt.AcceptChanges();

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgcheckResult.ItemsSource == null) return;

            DataTable dt = ((DataView)dgcheckResult.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }
    }
}
