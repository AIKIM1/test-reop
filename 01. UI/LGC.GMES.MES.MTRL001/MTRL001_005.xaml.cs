/*************************************************************************************
 Created Date : 2019.07.16
      Creator : 정문교
   Decription : 원자재관리 - 원자재 반품 적재 보고 Scan 
--------------------------------------------------------------------------------------
 [Change History]
  2019.07.16  정문교 : Initial Created.
  2019.10.08  정문교 : 반품사유 필수 입력으로 변경
  2019.11.08  정문교 : Biz 오류 메시지 다국어 처리 안됨
                       Biz 오류 메시지 Util.MessageValidation -> MessageException Call Back으로 변경
  2020.02.05  정문교 : IWMS <-> GMES I/F 변경에 따른 수정 
                       1.Biz 변경
                       2.요청 상태 코드 MTRL_SPLY_REQ_STAT_CODE -> MTRL_SPLY_REQ_STAT_CODE_ASSY 변경

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
using System.Windows.Threading;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_005 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_005()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            txtAGVID.Text = string.Empty;
            txtMLotID.Text = string.Empty;
        }

        private void InitializeGrid()
        {
            Util.gridClear(dgReturn);
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            // 반품사유
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: new string[] { "RETURN_MLOT" });
        }

        private void SetControl(bool isVisibility = false)
        {
            cboResnCode.SelectedValueChanged += cboResnCode_SelectedValueChanged;

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

                SearchAGV();
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
        /// 불량 사유 변경시
        /// </summary>
        private void cboResnCode_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            if (string.IsNullOrWhiteSpace(txtMLotID.Text))
            {
                txtMLotID.Focus();
            }
            else
            {
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
                    DataTable dt = DataTableConverter.Convert(dgReturn.ItemsSource);
                    DataRow dtRow = (dg.DataContext as DataRowView).Row;

                    dt.Select("MLOTID = '" + dtRow["MLOTID"] + "'").ToList<DataRow>().ForEach(row => row.Delete());
                    dt.AcceptChanges();
                    Util.GridSetData(dgReturn, dt, null);
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
            InitializeControls();
            InitializeGrid();
            SetControl();
        }

        /// <summary>
        /// 반품
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
            }, ObjectDic.Instance.GetObjectName("반품"));
        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// AGV 조회
        /// </summary>
        private void SearchAGV()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = txtAGVID.Text;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_EQUIPMENT_AGV", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult == null || bizResult.Rows.Count == 0)
                        {
                            //  스캔한 데이터가 없습니다.
                            Util.MessageValidation("SFU2067", result =>
                            {
                                txtAGVID.Text = string.Empty;
                                txtAGVID.Focus();
                            });
                            return;
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
            inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
            inTable.Columns.Add("S_BOX_ID", typeof(string));

            // INDATA SET
            DataRow newRow = inTable.NewRow();
            newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Return;
            newRow["S_BOX_ID"] = txtMLotID.Text;
            inTable.Rows.Add(newRow);

            return new ClientProxy().ExecuteServiceSync("BR_MTR_CHK_MLOT", "INDATA", "OUTDATA", inTable);
        }

        /// <summary>
        /// MLOT 조회
        /// </summary>
        private void SearchMLOT()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MLOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MLOTID"] = txtMLotID.Text;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_MLOT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            //내,외수 체크
                            DataTable dt = DataTableConverter.Convert(dgReturn.ItemsSource);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                if (dt.Rows[0]["MKT_TYPE_CODE"].ToString() != bizResult.Rows[0]["MKT_TYPE_CODE"].ToString())
                                {
                                    // 동일한 시장유형이 아닙니다.
                                    Util.MessageValidation("SFU4271", result =>
                                    {
                                        txtMLotID.Text = string.Empty;
                                        txtMLotID.Focus();
                                    });
                                    return;
                                }
                            }

                            bizResult.Columns.Add("RTN_RESNCODE");
                            bizResult.Columns.Add("RTN_RESNNAME");

                            if (cboResnCode.SelectedValue == null || cboResnCode.SelectedValue.ToString().Equals("SELECT"))
                            {
                                bizResult.Rows[0]["RTN_RESNCODE"] = null;
                                bizResult.Rows[0]["RTN_RESNNAME"] = string.Empty;
                            }
                            else
                            {
                                bizResult.Rows[0]["RTN_RESNCODE"] = cboResnCode.SelectedValue.ToString();
                                bizResult.Rows[0]["RTN_RESNNAME"] = cboResnCode.Text;
                            }

                            bizResult.AcceptChanges();
                            dt.Merge(bizResult);

                            Util.GridSetData(dgReturn, dt, null);
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
        /// 반품
        /// </summary>
        private void SaveProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                /////////////////////////////////////////////////////////////////  Table 생성
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AGV_ID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataTable inMtrl = inDataSet.Tables.Add("INMTRL");
                inMtrl.Columns.Add("MTRLID", typeof(string));
                inMtrl.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(string));

                DataTable inMLot = inDataSet.Tables.Add("INMLOT");
                inMLot.Columns.Add("MLOTID", typeof(string));
                inMLot.Columns.Add("MTRLID", typeof(string));
                inMLot.Columns.Add("MTRL_QTY", typeof(decimal));
                inMLot.Columns.Add("RTN_RESNCODE", typeof(string));

                /////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Return;
                newRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[0].DataItem, "EQPTID").ToString());
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AGV_ID"] = txtAGVID.Text;
                newRow["MTRL_SPLY_REQ_TYPE_CODE"] = Mtrl_Request_TypeCode.Return;
                newRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[0].DataItem, "MKT_TYPE_CODE").ToString());
                inTable.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(dgReturn.ItemsSource);
                // 자재별 Grouping
                var summarydata = from row in dt.AsEnumerable()
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
                    newRow["MTRL_SPLY_REQ_QTY"] = data.ReqQty;
                    inMtrl.Rows.Add(newRow);
                }

                foreach (DataRow row in dt.Rows)
                {
                    newRow = inMLot.NewRow();
                    newRow["MLOTID"] = Util.NVC(row["MLOTID"]);
                    newRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                    newRow["MTRL_QTY"] = Util.NVC(row["MLOTQTY_CUR"]);
                    newRow["RTN_RESNCODE"] = Util.NVC(row["RTN_RESNCODE"]);
                    inMLot.Rows.Add(newRow);
                }
                /////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_RETURN_NEW", "INDATA,INMTRL,INMLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 요청되었습니다.
                        Util.MessageValidation("SFU1747", result =>
                        {
                            TransferDataToMcs(bizResult.Tables["OUTDATA"].Rows[0]["MTRL_SPLY_REQ_ID"].ToString(),
                                              bizResult.Tables["OUTDATA"].Rows[0]["PORT_ID"].ToString());
                            // Clear
                            InitializeControls();
                            InitializeGrid();

                            txtAGVID.Focus();
                        });
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

        /// <summary>
        ///  MCS IP
        /// </summary>
        private DataTable SetMcsConfig()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("KEYGROUPID", typeof(string));

                //////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["KEYGROUPID"] = "MCS_AP_CONFIG";
                inTable.Rows.Add(newRow);

                //////////////////////////////////////////////////////////////////

                return new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        ///  MCS 데이터 전송
        /// </summary>
        private bool TransferDataToMcs(string Return_ReqID, string Port)
        {
            try
            {
                DataTable dtMCS = SetMcsConfig();

                if (dtMCS == null || dtMCS.Rows.Count == 0)
                {
                    // AMCS 연결정보를 설정 하세요.
                    Util.MessageValidation("SFU8026");
                    return false;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_LOCID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                //////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["CARRIERID"] = Return_ReqID;
                newRow["SRC_LOCID"] = txtAGVID.Text;
                //newRow["DST_LOCID"] = GetPortInfo();
                newRow["DST_LOCID"] = Port;
                newRow["UPDUSER"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                //////////////////////////////////////////////////////////////////
                string sIp = string.Empty;
                string sProtocol = string.Empty;
                string sPort = string.Empty;
                string sMode = string.Empty;
                string sId = string.Empty;
                //// 실전
                //sIp = "10.61.7.240";
                //sProtocol = "tcp";
                //sPort = "7885";
                //sMode = "SERVICE";
                //sId = "2";

                foreach (DataRow dr in dtMCS.Rows)
                {
                    if (dr["KEYID"].ToString() == "BizActorIP")
                        sIp = dr["KEYVALUE"].ToString();
                    else if (dr["KEYID"].ToString() == "BizActorPort")
                        sPort = dr["KEYVALUE"].ToString();
                    else if (dr["KEYID"].ToString() == "BizActorProtocol")
                        sProtocol = dr["KEYVALUE"].ToString();
                    else if (dr["KEYID"].ToString() == "BizActorServiceIndex")
                        sId = dr["KEYVALUE"].ToString();
                    else
                        sMode = dr["KEYVALUE"].ToString();
                }

                new ClientProxy(sIp, sProtocol, sPort, sMode, sId).ExecuteServiceSync("BR_GUI_REG_TRF_JOB_BYUSER", "IN_REQ_TRF_INFO", "OUT_REQ_TRF_INFO", inTable);

                return true;
            }
            catch (Exception ex)
            {
                return false;
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

        private bool ValidationSave()
        {
            if (string.IsNullOrWhiteSpace(txtAGVID.Text))
            {
                // AGV ID를 Scan하세요.
                Util.MessageValidation("SFU8003");
                return false;
            }

            if (dgReturn.Rows.Count == 0)
            {
                // 입력된 항목이 없습니다.
                Util.MessageValidation("SFU2052");
                return false;
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
                if (cboResnCode.SelectedValue == null || cboResnCode.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 반품사유를 입력하세요
                    Util.MessageValidation("SFU1554", result =>
                    {
                        cboResnCode.Focus();
                    });
                    return;
                }

                if (dgReturn.Rows.Count > 0)
                {
                    DataRow[] dr = DataTableConverter.Convert(dgReturn.ItemsSource).Select("MLOTID = '" + txtMLotID.Text + "'");
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
                }

                DataTable dt = SearchScanValue();
                /////////////////////////////////////////////////////////////////////////////////////////////////

                if (dt != null && dt.Rows.Count > 0)
                {
                    SearchMLOT();
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
