/*************************************************************************************
 Created Date : 2020.08.20
      Creator : 정문교
   Decription : 원자재관리 - 원자재 요청 수동입력 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2019.07.05  정문교 : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_001_TAKEOVER : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        string _ReqID;

        private bool _load = true;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public MTRL001_001_TAKEOVER()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetControlVisibility();

                //  요청 조회
                SearchProcess();

                _load = false;
            }

        }
        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _ReqID = tmps[0] as string;
            txtReqID.Text = _ReqID;
        }

        private void SetControlVisibility()
        {
        }

        #endregion

        #region Event

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
                    DataTable dt = DataTableConverter.Convert(dgRequest.ItemsSource);
                    DataRow dtRow = (dg.DataContext as DataRowView).Row;

                    dt.Select("MLOTID = '" + dtRow["MLOTID"] + "'").ToList<DataRow>().ForEach(r => r["CNFM_QTY"] = 0);
                    dt.AcceptChanges();
                    Util.GridSetData(dgRequest, dt, null);
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
            DataTable dt = DataTableConverter.Convert(dgRequest.ItemsSource);

            dt.Select().ToList<DataRow>().ForEach(r => r["CNFM_QTY"] = 0);
            dt.AcceptChanges();
            Util.GridSetData(dgRequest, dt, null);
        }

        /// <summary>
        /// 인수확정
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;

            // %1 (을)를 하시겠습니까?
            Util.MessageConfirm("SFU4329", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            }, ObjectDic.Instance.GetObjectName("인수"));
            //}
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region User Method

        #region [BizCall]
        /// <summary>
        /// 요청 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_ID"] = _ReqID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_AGV_TRANSFER_MLOT_NEW", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
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

                        txtMLotID.Focus();
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
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inMtrl = inDataSet.Tables.Add("INMTRL");
                inMtrl.Columns.Add("MTRLID", typeof(string));
                inMtrl.Columns.Add("CNFM_QTY", typeof(string));

                DataTable inMLot = inDataSet.Tables.Add("INMLOT");
                inMLot.Columns.Add("MLOTID", typeof(string));
                inMLot.Columns.Add("MTRLID", typeof(string));

                /////////////////////////////////////////////////////////////////
                DataRow[] dr = DataTableConverter.Convert(dgRequest.ItemsSource).Select("CNFM_QTY > 0");

                DataRow newRow = inTable.NewRow();
                newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(dr[0]["MTRL_SPLY_REQ_ID"].ToString());
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.TakingOver;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                // 자재별 Grouping
                var summarydata = from row in dr.AsEnumerable()
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
                foreach (DataRow row in dr)
                {
                    newRow = inMLot.NewRow();
                    newRow["MLOTID"] = Util.NVC(row["MLOTID"]);
                    newRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                    inMLot.Rows.Add(newRow);
                }
                /////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_TAKING_OVER", "INDATA,INMTRL,INMLOT", null, (bizResult, bizException) =>
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

                        this.DialogResult = MessageBoxResult.OK;
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

        #region[[Validation]
        private bool ValidationSave()
        {
            if (dgRequest.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgRequest.ItemsSource).Select("CNFM_QTY > 0");

            if (dr.Length == 0)
            {
                // 스캔한 데이터가 없습니다.
                Util.MessageValidation("SFU2060");
                return false;
            }

            return true;
        }
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
                        txtMLotID.Text = string.Empty;
                        txtMLotID.Focus();
                    });
                    return;
                }

                DataRow[] dr = DataTableConverter.Convert(dgRequest.ItemsSource).Select("MLOTID = '" + txtMLotID.Text + "' And CNFM_QTY > 0");

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


                // 자재별 합산 조회
                CountLoadMtrl(txtMLotID.Text);

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

        private void CountLoadMtrl(string MLotID)
        {
            DataTable dtMtrl = DataTableConverter.Convert(dgRequest.ItemsSource);

            // 확인수량에 Update
            dtMtrl.Select("MLOTID ='" + MLotID + "'").ToList<DataRow>().ForEach(r => r["CNFM_QTY"] = 1);
            dtMtrl.AcceptChanges();
            Util.GridSetData(dgRequest, dtMtrl, null);
        }
        #endregion

        #endregion

    }
}
