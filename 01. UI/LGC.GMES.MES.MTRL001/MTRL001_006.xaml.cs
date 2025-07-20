/*************************************************************************************
 Created Date : 2019.07.17
      Creator : 정문교
   Decription : 원자재관리 - 원자재 반품 인수 확정
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.08  정문교 : Biz 오류 메시지 다국어 처리 안됨
                       Biz 오류 메시지 Util.MessageValidation -> MessageException Call Back으로 변경


 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using System.Windows.Threading;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        bool _DiffQty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_006()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            _DiffQty = false;
            txtAGVID.Text = string.Empty;
            txtMLotID.Text = string.Empty;
        }

        private void InitializeGrid()
        {
            Util.gridClear(dgRequest);
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
        }

        private void SetControl(bool isVisibility = false)
        {
            txtAGVID.Focus();
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void dgRequest_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("MTRL_SPLY_REQ_QTY") || e.Cell.Column.Name.Equals("CNFM_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        if (e.Cell.Column.Name.Equals("CNFM_QTY"))
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        else
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        /// <summary>
        /// AGV Scan
        /// /// </summary>
        private void txtAGVID_GotFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke
            (
                DispatcherPriority.ContextIdle,
                new Action
                (
                    delegate {(sender as TextBox).SelectAll();}
                )
            );
        }

        private void txtAGVID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch()) return;

                SearchProcess();
            }
        }

        /// <summary>
        /// 자재LOT Scan
        /// /// </summary>
        private void txtMLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtMLotID.Text)) return;

                CheckScanValueMLOT();
            }
        }

        /// <summary>
        /// Scan 자료 삭제
        /// /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button dg = sender as Button;
                if (dg != null &&
                    dg.DataContext != null &&
                    (dg.DataContext as DataRowView).Row != null)
                {
                    DataTable dtRequest = DataTableConverter.Convert(dgRequest.ItemsSource);
                    DataRow dtRow = (dg.DataContext as DataRowView).Row;

                    dtRequest.Select("MLOTID = '" + dtRow["MLOTID"].ToString() + "'").ToList<DataRow>().ForEach(r => r["CNFM_QTY"] = 0);
                    dtRequest.AcceptChanges();
                    Util.GridSetData(dgRequest, dtRequest, null, true); ;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRequest = DataTableConverter.Convert(dgRequest.ItemsSource);
            dtRequest.Select("").ToList<DataRow>().ForEach(r => r["CNFM_QTY"] = 0);
            dtRequest.AcceptChanges();
            Util.GridSetData(dgRequest, dtRequest, null, true);
        }

        /// <summary>
        /// 인수확정
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;

            if (_DiffQty)
            {
                // 요청수량과 확인수량이 틀립니다. \r\n 인수확정 하시겠습니까?
                Util.MessageConfirm("SFU8006", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveProcess();
                    }
                });
            }
            else
            {
                // %1 (을)를 하시겠습니까?
                Util.MessageConfirm("SFU4329", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveProcess();
                    }
                }, ObjectDic.Instance.GetObjectName("인수"));
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 반품 요청 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AGV_ID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AGV_ID"] = txtAGVID.Text;
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Transfer;
                newRow["MTRL_SPLY_REQ_TYPE_CODE"] = Mtrl_Request_TypeCode.Return;
                inTable.Rows.Add(newRow);

                // Clear
                InitializeGrid();

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_AGV_TRANSFER_MLOT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgRequest, bizResult, FrameOperation, true);

                        if (bizResult == null || bizResult.Rows.Count == 0)
                        {
                            txtAGVID.SelectAll();
                            txtAGVID.Focus();
                        }
                        else
                        {
                            txtMLotID.Focus();
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Scan Value 체크
        /// </summary>
        private DataTable SearchScanValue()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
            inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
            inTable.Columns.Add("S_BOX_ID", typeof(string));
            inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));

            // INDATA SET
            DataRow newRow = inTable.NewRow();
            newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[0].DataItem, "MTRL_SPLY_REQ_ID").ToString());
            newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Completed;
            newRow["S_BOX_ID"] = txtMLotID.Text;
            newRow["MTRL_SPLY_REQ_TYPE_CODE"] = Mtrl_Request_TypeCode.Return;
            inTable.Rows.Add(newRow);

            return new ClientProxy().ExecuteServiceSync("BR_MTR_CHK_PALLET_MLOT", "INDATA", "OUTDATA", inTable);
        }

        /// <summary>
        /// 인수확정
        /// </summary>
        private void SaveProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                /////////////////////////////////////////////////////////////////  Table 생성
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AGV_ID", typeof(string));

                DataTable inMtrl = inDataSet.Tables.Add("INMTRL");
                inMtrl.Columns.Add("MTRLID", typeof(string));
                inMtrl.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(Int16));
                inMtrl.Columns.Add("CNFM_QTY", typeof(Int16));

                DataTable inMLot = inDataSet.Tables.Add("INMLOT");
                inMLot.Columns.Add("MLOTID", typeof(string));
                inMLot.Columns.Add("MTRLID", typeof(string));
                inMLot.Columns.Add("MTRL_QTY", typeof(decimal));

                /////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[0].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Completed;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] drRequest = DataTableConverter.Convert(dgRequest.ItemsSource).Select("CNFM_QTY <> '0'");

                // 자재별 Grouping
                var summarydata = from row in drRequest.AsEnumerable()
                                  group row by new
                                  {
                                      MtrlID = row.Field<string>("MTRLID"),
                                  } into grp
                                  select new
                                  {
                                      MtrlID = grp.Key.MtrlID,
                                      ReqQty = grp.Count()
                                  };

                foreach (var data in summarydata)
                {
                    newRow = inMtrl.NewRow();
                    newRow["MTRLID"] = data.MtrlID;
                    newRow["CNFM_QTY"] = data.ReqQty;
                    inMtrl.Rows.Add(newRow);
                }

                // 자재 LOT
                foreach (DataRow row in drRequest)
                {
                    newRow = inMLot.NewRow();
                    newRow["MLOTID"] = row["MLOTID"];
                    inMLot.Rows.Add(newRow);
                }
                /////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_RETURN", "INDATA,INMTRL,INMLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 인수되었습니다.
                        Util.MessageInfo("SFU1793");

                        // Clear
                        InitializeControls();
                        InitializeGrid();

                        txtAGVID.Focus();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Function

        #region [Validation]
        private bool ValidationSearch()
        {
            if (string.IsNullOrWhiteSpace(txtAGVID.Text))
            {
                // AGV ID를 Scan하세요.
                Util.MessageValidation("SFU8003");
                return false;
            }

            return true;
        }

        private bool ValidationRejection()
        {
            if (dgRequest.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            _DiffQty = false;

            if (dgRequest.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            DataTable dt = DataTableConverter.Convert(dgRequest.ItemsSource);
            DataRow[] drQty = dt.Select("CNFM_QTY <> 0");

            if (drQty.Length == 0)
            {
                // 스캔한 데이터가 없습니다.
                Util.MessageValidation("SFU2060");
                return false;
            }

            DataRow[] dr = dt.Select("REQ_QTY <> CNFM_QTY");

            if (dr.Length > 0)
            {
                _DiffQty = true;
            }

            return true;
        }

        #endregion

        #region [팝업]
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

        /// <summary>
        /// 자재LOT Scan 체크
        /// </summary>
        private void CheckScanValueMLOT()
        {
            try
            {
                if (dgRequest.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645", result =>
                    {
                        txtAGVID.Text = string.Empty;
                        txtAGVID.Focus();
                    });
                    return;
                }

                DataTable dtRequest = DataTableConverter.Convert(dgRequest.ItemsSource);
                DataRow[] dr = dtRequest.Select("MLOTID =  '" + txtMLotID.Text + "' AND CNFM_QTY <> '0'");

                if (dr.Length > 0)
                {
                    // 이미 바코드 스캔 완료하였습니다.
                    Util.MessageValidation("SFU5114", result =>
                    {
                        txtMLotID.Text = string.Empty;
                        txtMLotID.Focus();
                    });
                    return;
                }

                DataTable dt = SearchScanValue();
                /////////////////////////////////////////////////////////////////////////////////////////////////

                if (dt != null && dt.Rows.Count > 0)
                {
                    dtRequest.Select("MLOTID = '" + txtMLotID.Text + "'").ToList<DataRow>().ForEach(r => r["CNFM_QTY"] = 1);
                    dtRequest.AcceptChanges();
                    Util.GridSetData(dgRequest, dtRequest, null, true); ;
                }

                txtMLotID.Text = string.Empty;
                txtMLotID.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, result =>
                {
                    txtMLotID.Text = string.Empty;
                    txtMLotID.Focus();
                });

                return;
            }
        }

        #endregion

        #endregion

        #endregion

    }
}
