/*************************************************************************************
 Created Date : 2021.05.11
      Creator : 오화백
   Decription : 고공 CNV 현황 조회 - 실 Tray 출고 예약 [팝업]
--------------------------------------------------------------------------------------
 [Change History]
  2021.05.11  오화백 : Initial Created.    
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
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;



namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_060_USING_TRAY_OUTPUT : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;
        private string _EqptID = string.Empty;// 설비정보

        public bool IsUpdated;

        private readonly Util _util = new Util();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //GetBizActorServerInfo();// 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
        }
        public MCS001_060_USING_TRAY_OUTPUT()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        /// <summary>
        /// 화면로드시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
           object[] parameters = C1WindowExtension.GetParameters( this );
           _EqptID = parameters.Length >= 1 ? Util.NVC(parameters[0]) : string.Empty;  //설비 정보
            InitializeCombo();

        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnOutput};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitializeCombo()
        {
            SetOutPutLoc(cboOutputLoc, CommonCombo.ComboStatus.SELECT);           // 출고위치
            SetOutPutPort(cboOutputPort);                                        // 출고포트 
            SetOutPutPort(cboLine);                                              // 라인 
        }



        #endregion

        #region Event


        #region 출고위치 콤보박스 이벤트 : cboOutputLoc_SelectedValueChanged()
        /// <summary>
        /// 출고 위치 콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboOutputLoc_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetOutPutPort(cboOutputPort);
            SetOutPutPort(cboLine);
        }

        #endregion

        #region 출고 Port 콤보박스 이벤트 :  cboOutputPort_SelectedValueChanged()
        private void cboOutputPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            // 출고 Port에 따른 라인 정보 조회 
            if (cboOutputPort.SelectedValue != null)
            {
                cboLine.SelectedValue = cboOutputPort.SelectedValue;
            }
        }
        #endregion

        #region 조회버튼 클릭 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearch()) return;

                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_GET_HIGH_CNV_U_TRAY_OUT_LIST_DETL";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TYPE", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _EqptID;
                dr["TYPE"] = cboOutputLoc.SelectedValue.ToString();
                if(cboOutputLoc.SelectedValue.ToString() == "PKG")
                {
                    dr["EQSGID"] = cboLine.Text.ToString();
                }
                else
                {
                    dr["EQSGID"] = null;
                }
                   
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, null, true);

                        dgList.MergingCells -= dgList_MergingCells;
                        dgList.MergingCells += dgList_MergingCells;

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
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 스프레드 머지 (Rack에 따른 체크박스머지) : dgList_MergingCells()
        /// <summary>
        /// 스프레드 머지 (Rack에 따른 체크박스머지)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count; i++)
                {

                    if (dgList.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "RACK_NAME"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "RACK_NAME")).Equals(sTmpLvCd))
                            {
                                idxE = i;

                                //마지막 Row 일경우
                                if (i == dgList.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CHK"].Index), dgList.GetCell(idxE, dgList.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["REQ_STAT"].Index), dgList.GetCell(idxE, dgList.Columns["REQ_STAT"].Index)));

                                }

                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CHK"].Index), dgList.GetCell(idxE, dgList.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["REQ_STAT"].Index), dgList.GetCell(idxE, dgList.Columns["REQ_STAT"].Index)));
                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "RACK_NAME"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 출고예약 버튼 클릭 : btnOutput_Click()
        /// <summary>
        /// 출고예약 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationOutput()) return;

            // 출고예약 하시겠습니까?
            Util.MessageConfirm("SFU8360", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    OutPut();
                }
            });
        }
        #endregion

        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #endregion

        #region Mehod

        #region 출고우치 콤보박스 조회 : SetCommonCombo()
        /// <summary>
        /// 출고위치 조회
        /// </summary>
      
        private void SetOutPutLoc(C1ComboBox cbo, CommonCombo.ComboStatus status)
        {
            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_DST_LIST";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
        

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = _EqptID;
        
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { cbo.SelectedValuePath, cbo.DisplayMemberPath });

            DataRow newRow = dtBinding.NewRow();

            switch (status)
            {
                case CommonCombo.ComboStatus.ALL:
                    newRow[cbo.SelectedValuePath] = null;
                    newRow[cbo.DisplayMemberPath] = "-ALL-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    newRow[cbo.SelectedValuePath] = "SELECT";
                    newRow[cbo.DisplayMemberPath] = "-SELECT-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    newRow[cbo.SelectedValuePath] = string.Empty;
                    newRow[cbo.DisplayMemberPath] = "-N/A-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.NONE:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;

        }
        #endregion

        #region 출고 Port 콤보박스 조회 : SetMachineCombo()
        /// <summary>
        /// 출고 PORT 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetOutPutPort(C1ComboBox cbo)
        {
            string outputLoc = string.IsNullOrEmpty(cboOutputLoc?.SelectedValue.GetString()) ? null : cboOutputLoc?.SelectedValue.GetString();
        

            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_DST_LIST_DETL";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("TYPE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = _EqptID;
            dr["TYPE"] = outputLoc;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataTable dtBinding = dtResult.Copy();
            DataRow newRow = dtBinding.NewRow();

            newRow[cbo.SelectedValuePath] = null;
            newRow[cbo.DisplayMemberPath] = "-SELECT-";
            dtBinding.Rows.InsertAt(newRow, 0);

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }
        #endregion
        
        #region 조회 Validation : ValidationSearch()

        /// <summary>
        ///  조회 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationSearch()
        {
            if (cboOutputLoc.SelectedIndex == 0)
            {
                // 출고위치 (을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고위치"));
                return false;
            }

            if (cboOutputPort.SelectedIndex == 0)
            {
                //출고port를 선택하세요
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고Port"));
                return false;
            }
            return true;
        }

        #endregion

        #region 출고처리 Validation : ValidationOutput()
        /// <summary>
        /// 출고처리 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationOutput()
        {

            if (cboOutputLoc.SelectedIndex == 0)
            {
                // 출고위치 (을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고위치"));
                return false;
            }

            if (cboOutputPort.SelectedIndex == 0)
            {
                //출고port를 선택하세요
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고Port"));
                return false;
            }

            if (dgList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다.
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgList, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgList))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }


            return true;
        }

        #endregion

        #region 출고처리 : OutPut()
        /// <summary>
        /// 출고처리
        /// </summary>
        private void OutPut()
        {

            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER";

                DateTime dtSystem = GetSystemTime();

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_LOCID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("DTTM", typeof(DateTime));


                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = GetFirstCstid(_EqptID, DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString());
                        newRow["SRC_LOCID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                        newRow["DST_LOCID"] = cboOutputPort.SelectedValue.ToString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["DTTM"] = dtSystem;
                        inTable.Rows.Add(newRow);
                    }
                }
                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", "OUT_REQ_TRF_INFO", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        IsUpdated = true;
                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다
                        btnSearch_Click(btnSearch, null);
                        //this.DialogResult = MessageBoxResult.OK;
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
                Util.MessageException(ex);
            }
           
        }

        #endregion

        #region 프로그래스 관련 Method : ShowLoadingIndicator(), HiddenLoadingIndicator()
        /// <summary>
        /// 프로그래스 바 보이기
        /// </summary>
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 프로그래스 바 숨기기
        /// </summary>
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region MCS 비즈 접속 정보 : GetBizActorServerInfo()
        /// <summary>
        /// MCS 비즈 접속 정보
        /// </summary>
        private void GetBizActorServerInfo()
        {

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "MCS_AP_CONFIG";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }

        }



        #endregion

        #region 출고예약 시  등록일자 셋팅 : GetSystemTime()
        /// <summary>
        /// 등록일자 셋팅
        /// </summary>
        /// <returns></returns>
        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }
        #endregion

        #region 출고예약 시 해당 RACK의 첫번째 CSTID 값 셋팅 : GetFirstCstid()
        /// <summary>
        /// CSTID 조회
        /// </summary>
        /// <returns></returns>
        private string GetFirstCstid(string Eqptid, string RackID)
        {
            string FirstCstid = string.Empty;

            const string bizRuleName = "DA_GMES_CELL_TRAY_BUNDLE_INFO_BY_RACKID";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("RACK_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = Eqptid;
            dr["RACK_ID"] = RackID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                FirstCstid = dtResult.Rows[0]["CSTID"].ToString();
            }

            return FirstCstid;
        }

        #endregion

        #endregion




    }
}