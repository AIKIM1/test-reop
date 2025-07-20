/*************************************************************************************
 Created Date : 2019.05.21
      Creator : 김대근
   Decription : Carrier -Lot 연계, 연계해제 관리
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.21  김대근 : Initial Created.   
  2020.06.11  정문교 : Carrier -Lot 연계, 연계해제시 인증 팝업 추가(오화백 과장 소스 원복)
  2020.09.18  신광희 : CSR C20200917-000434, 공통코드에 등록된 동 기준 사용자 인증 팝업 호출 적용
  2021.12.01  정재홍 : C20211002-000028, GMES전극시스템 Carrier mapping error Bobbin 창고 이송불가 interlock 요청
  2023.12.21  양영재 : E20231031-000680, 특이작업 - Carrier-Lot 정보관리 - '연계' 란에서 저장 버튼 클릭 시 'CARRIER_LOT_MANAGER_AUTH' 동별 공통코드에 따라 인증 팝업 호출
  2024.04.04  오화백 : Carrier ID  대문자만 입력 되도록 수정
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_260_CST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_260_CST : UserControl, IWorkArea
    {
        #region [공통]
        private DataTable resultTable;
        private DataTable dtLock;
        private DataRow tmpDataRow;

        private string _UserID = string.Empty; //직접 실행하는 USerID
        private string _authorityGroup = string.Empty;

        bool isInterLock = false;

        private enum TYPE
        {
            MAPPING = 0,
            EMPTY = 1,
            LOCK  = 2
        }

        public COM001_260_CST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }

        private void Initialize()
        {
            // MES 2.0 - 비정상 Carrier Interlock 해제 Tab 조립의 경우, 미표시.
            CheckAbCstInterlockUse(LoginInfo.CFG_AREA_ID);
        }

        private void InitializeResultTable()
        {
            if (resultTable == null)
            {
                resultTable = new DataTable();
                resultTable.Columns.Add("CHK", typeof(bool));
                resultTable.Columns.Add("AREANAME", typeof(string));
                resultTable.Columns.Add("AREAID", typeof(string));
                resultTable.Columns.Add("CSTTYPE", typeof(string));
                resultTable.Columns.Add("CSTID", typeof(string));
                resultTable.Columns.Add("CSTSTAT", typeof(string));
                resultTable.Columns.Add("LOTID", typeof(string));
                resultTable.Columns.Add("INSUSER", typeof(string));
                resultTable.Columns.Add("INSDTTM", typeof(DateTime));
                resultTable.Columns.Add("UPDUSER", typeof(string));
                resultTable.Columns.Add("UPDDTTM", typeof(DateTime));
                resultTable.Columns.Add("CSTTNAME", typeof(string));
                resultTable.Columns.Add("CSTSNAME", typeof(string));
                resultTable.Columns.Add("PORTNAME", typeof(string));
            }
        }

        private void CheckCarrierInfo(TextBox _txtCstID, TextBox _txtLotID, TYPE _type)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CSTID"] = _txtCstID.Text.Trim();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_CARRIER", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //캐리어 존재여부
                        if (searchResult.Rows.Count == 0)
                        {
                            object[] parameters = new object[1];
                            parameters[0] = _txtCstID.Text.Trim();
                            MessageValidationWithCallBack("SFU7001", (result) =>
                            {
                                if (_type == TYPE.MAPPING)
                                {
                                    _txtLotID.Clear();
                                    _txtLotID.IsReadOnly = true;
                                }
                                _txtCstID.Focus();
                            }, parameters); //CSTID[%1]에 해당하는 CST가 없습니다.

                            return;
                        }

                        //캐리어 상태
                        switch (_type)
                        {
                            case TYPE.MAPPING:
                                if (Util.NVC(searchResult.Rows[0]["CSTSTAT"]).Equals("U"))
                                {
                                    object[] parameters = new object[2];
                                    parameters[0] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                                    parameters[1] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);
                                    MessageValidationWithCallBack("SFU7002", (result) =>
                                    {
                                        _txtCstID.Focus();

                                        _txtLotID.Clear();
                                        _txtLotID.IsReadOnly = true;
                                    }, parameters); //CSTID[%1] 이 상태가 %2 입니다.

                                    return;
                                }
                                break;

                            case TYPE.EMPTY:
                                if (Util.NVC(searchResult.Rows[0]["CSTSTAT"]).Equals("E"))
                                {
                                    object[] parameters = new object[2];
                                    parameters[0] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                                    parameters[1] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);
                                    MessageValidationWithCallBack("SFU7002", (result) =>
                                    {
                                        _txtCstID.Focus();

                                        _txtLotID.Clear();
                                        _txtLotID.IsReadOnly = true;
                                    }, parameters); //CSTID[%1] 이 상태가 %2 입니다.

                                    return;
                                }
                                break;

                            //C20211002-000028
                            case TYPE.LOCK:
                                if (!Util.NVC(searchResult.Rows[0]["MAPP_ERR_OCCR_FLAG"]).Equals("Y"))
                                {
                                    object[] parameters = new object[2];
                                    parameters[0] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                                    parameters[1] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);
                                    MessageValidationWithCallBack("SFU8433", (result) =>
                                    {
                                        _txtCstID.Focus();
                                        _txtCstID.Clear();
                                        
                                    }, parameters); //해당 Carrier는 정상입니다.

                                    return;
                                }
                                break;
                        }

                        //에러 없는 경우
                        tmpDataRow = null;
                        tmpDataRow = resultTable.NewRow();
                        tmpDataRow["CSTTYPE"] = Util.NVC(searchResult.Rows[0]["CSTTYPE"]);
                        tmpDataRow["CSTID"] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                        tmpDataRow["CSTSTAT"] = Util.NVC(searchResult.Rows[0]["CSTSTAT"]);
                        tmpDataRow["CSTTNAME"] = Util.NVC(searchResult.Rows[0]["CSTTNAME"]);
                        tmpDataRow["CSTSNAME"] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);

                        if (_type == TYPE.MAPPING)
                        {
                            _txtLotID.Clear();
                            _txtLotID.IsReadOnly = false;
                            _txtLotID.Focus();
                        }
                        else if (_type == TYPE.EMPTY)
                        {
                            _txtLotID.Text = Util.NVC(searchResult.Rows[0]["CURR_LOTID"]);
                            CheckLotInfo(_txtCstID, _txtLotID, TYPE.EMPTY);
                        }
                        //C20211002-000028
                        else if (_type == TYPE.LOCK)
                        {

                            tmpDataRow["CHK"] = false;
                            tmpDataRow["AREANAME"] = LoginInfo.CFG_AREA_NAME;
                            tmpDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                            tmpDataRow["LOTID"] = Util.NVC(searchResult.Rows[0]["CURR_LOTID"]);
                            tmpDataRow["PORTNAME"] = Util.NVC(searchResult.Rows[0]["PORTNAME"]);
                            tmpDataRow["INSUSER"] = LoginInfo.USERID;
                            tmpDataRow["INSDTTM"] = DateTime.Now;
                            tmpDataRow["UPDUSER"] = LoginInfo.USERID;
                            tmpDataRow["UPDDTTM"] = tmpDataRow["INSDTTM"];
                            resultTable.Rows.Add(tmpDataRow);

                            Util.GridSetData(dgInterlock, resultTable, this.FrameOperation);

                            _txtCstID.Focus();
                            _txtCstID.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CheckAbCstInterlockUse(string _areaID)
        {

            const string bizRuleName = "DA_BAS_SEL_AREA";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dtRow = inTable.NewRow();
            dtRow["AREAID"] = _areaID ?? null;
            inTable.Rows.Add(dtRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                for (int row = 0; row < dtResult.Rows.Count; row++)
                {
                    if (dtResult.Rows[row]["AREA_TYPE_CODE"].ToString().Equals("A"))
                    {
                        TabInterlock.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void CheckLotInfo(TextBox _txtCstID, TextBox _txtLotID, TYPE _type)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _txtLotID.Text.Trim();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_VW_WIP", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //데이터 없는 경우
                        if (searchResult.Rows.Count == 0)
                        {
                            object[] parameters = new object[1];
                            parameters[0] = _txtLotID.Text.Trim();
                            MessageValidationWithCallBack("SFU7000", (result) =>
                            {
                                if (_type == TYPE.MAPPING)
                                {
                                    _txtLotID.Focus();
                                }
                                else if (_type == TYPE.EMPTY)
                                {
                                    _txtLotID.Clear();

                                    _txtCstID.Clear();
                                    _txtCstID.Focusable = true;
                                    _txtCstID.Focus();
                                }
                            }, parameters); //LOTID[%1]에 해당하는 LOT이 없습니다.

                            return;
                        }

                        //재공상태 - 이동중인 상태만 막음.
                        //C20200518-000490
                        if (Util.NVC(searchResult.Rows[0]["WIPSTAT"]).Equals("MOVING"))
                        //||  Util.NVC(searchResult.Rows[0]["WIPSTAT"]).Equals("TERM"))
                        {
                            Util.MessageValidation("SFU2063", (result) =>
                            {
                                if (_type == TYPE.MAPPING)
                                {
                                    _txtLotID.Clear();
                                    _txtLotID.IsReadOnly = false;
                                    _txtLotID.Focusable = true;
                                    _txtLotID.Focus();
                                }
                                else if (_type == TYPE.EMPTY)
                                {
                                    _txtLotID.Clear();

                                    _txtCstID.Clear();
                                    _txtCstID.Focusable = true;
                                    _txtCstID.Focus();
                                }
                            }); //재공상태를 확인해주세요.

                            return;
                        }

                        /*
                        //재공상태
                        if (!Util.NVC(searchResult.Rows[0]["WIPSTAT"]).Equals("WAIT")
                        && !Util.NVC(searchResult.Rows[0]["WIPSTAT"]).Equals("EQPT_END"))
                        {
                            Util.MessageValidation("SFU2063", (result) =>
                            {
                                if (_type == TYPE.MAPPING)
                                {
                                    _txtLotID.Clear();
                                    _txtLotID.IsReadOnly = false;
                                    _txtLotID.Focusable = true;
                                    _txtLotID.Focus();
                                }
                                else if (_type == TYPE.EMPTY)
                                {
                                    _txtLotID.Clear();

                                    _txtCstID.Clear();
                                    _txtCstID.Focusable = true;
                                    _txtCstID.Focus();
                                }
                            }); //재공상태를 확인해주세요.

                            return;
                        }
                        */

                        //연계된 CSTID가 있는 경우
                        if (!string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CSTID"])) && _type == TYPE.MAPPING)
                        {
                            object[] parameters = new object[2];
                            parameters[0] = Util.NVC(searchResult.Rows[0]["LOTID"]);
                            parameters[1] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                            MessageValidationWithCallBack("SFU7003", (result) =>
                            {
                                _txtLotID.Focus();
                            }, parameters); //LOTID[%1]에 연계된 CSTID[%2]가 있습니다.

                            return;
                        }

                        // Lot이 HOLD 상태인 경우
                        if (Util.NVC(searchResult.Rows[0]["WIPHOLD"]).Equals("Y"))
                        {
                            object[] parameters = new object[1];
                            parameters[0] = _txtLotID.Text.Trim();
                            MessageValidationWithCallBack("SFU1761", (result) =>
                            {
                                if (_type == TYPE.MAPPING)
                                {
                                    _txtLotID.Focus();
                                }
                                else if (_type == TYPE.EMPTY)
                                {
                                    _txtLotID.Clear();

                                    _txtCstID.Clear();
                                    _txtCstID.Focusable = true;
                                    _txtCstID.Focus();
                                }
                            }, parameters); //LOTID[%1]이 HOLD 상태 입니다.

                            return;
                        }

                        //에러가 없는 경우
                        tmpDataRow["CHK"] = false;
                        tmpDataRow["AREANAME"] = LoginInfo.CFG_AREA_NAME;
                        tmpDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                        tmpDataRow["LOTID"] = Util.NVC(searchResult.Rows[0]["LOTID"]);
                        tmpDataRow["INSUSER"] = LoginInfo.USERID;
                        tmpDataRow["INSDTTM"] = DateTime.Now;
                        tmpDataRow["UPDUSER"] = LoginInfo.USERID;
                        tmpDataRow["UPDDTTM"] = tmpDataRow["INSDTTM"];
                        resultTable.Rows.Add(tmpDataRow);

                        if (_type == TYPE.MAPPING)
                        {
                            _txtCstID.Clear();

                            _txtLotID.Clear();
                            _txtLotID.IsReadOnly = true;

                            Util.GridSetData(dgMapping, resultTable, this.FrameOperation);
                        }
                        else if (_type == TYPE.EMPTY)
                        {
                            _txtCstID.Clear();
                            _txtCstID.Focusable = true;
                            _txtCstID.Focus();

                            Util.GridSetData(dgEmpty, resultTable, this.FrameOperation);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CanAddToRow(DataTable dt, TextBox txt, string value)
        {
            bool canAdd = true;

            foreach (DataRowView drv in dt.DefaultView)
            {
                if (txt.Text.Trim().Equals(Util.NVC(DataTableConverter.GetValue(drv, value))))
                {
                    canAdd = false;

                    object[] parameters = new object[1];
                    if (value.Equals("CSTID"))
                        parameters[0] = ObjectDic.Instance.GetObjectName("CSTID") + "[" + Util.NVC(txt.Text) + "]";
                    else if (value.Equals("LOTID"))
                        parameters[0] = ObjectDic.Instance.GetObjectName("LOTID") + "[" + Util.NVC(txt.Text) + "]";

                    MessageValidationWithCallBack("SFU3471", (result) =>
                    {
                        txt.Clear();
                        txt.Focus();
                    }, parameters);

                    break;
                }
            }

            return canAdd;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private bool IsOpenPopupAuthorityConfirmByArea()
        {
            _authorityGroup = string.Empty;

            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CST_LOT_MGMT_AUTH_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    _authorityGroup = dtResult.Rows[0]["ATTRIBUTE1"].GetString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }

        public static void MessageValidationWithCallBack(string messageId, Action<MessageBoxResult> callback, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, callback);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeResultTable();
            //tmpDataRow = null;
            Initialize();
        }

        private void btnDelRow_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            resultTable.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, resultTable, this.FrameOperation);
        }

        private void ClearAll(TextBox cstID, TextBox lotID, C1DataGrid dg)
        {
            InitializeResultTable();
            tmpDataRow = null;

            cstID.Clear();
            cstID.Focusable = true;
            cstID.Focus();

            lotID.Clear();
            lotID.IsReadOnly = true;

            resultTable.Clear();

            Util.gridClear(dg);
        }
        #endregion

        #region [연계처리]
        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (!CanAddToRow(resultTable, txtCSTID, "CSTID"))
                return;

            CheckCarrierInfo(txtCSTID, txtLOTID, TYPE.MAPPING);
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (!CanAddToRow(resultTable, txtLOTID, "LOTID"))
                return;

            CheckLotInfo(txtCSTID, txtLOTID, TYPE.MAPPING);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (resultTable.Rows.Count == 0 || dgMapping.Rows.Count == 0)
            {
                Util.MessageValidation("MMD0002");
                return;
            }

            if(IsOpenPopupAuthorityConfirmByArea() && LoginInfo.USERTYPE == "P")    //공통코드에 등록된 동 기준 && 공정PC 인 경우 사용자 인증 팝업 호출
            {
                CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new CMM001.CMM_COM_AUTH_CONFIRM();
                authConfirm.FrameOperation = FrameOperation;
                if (authConfirm != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = _authorityGroup;
                    Parameters[1] = "lgchem.com";
                    C1WindowExtension.SetParameters(authConfirm, Parameters);
                    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Mapping);

                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(authConfirm);
                            authConfirm.BringToFront();
                            break;
                        }
                    }
                }
            }
            //  [E20231031-000680] : 'CARRIER_LOT_MANAGER_AUTH' 동별 공통코드에 따라 인증 팝업 호출
            if (IsAreaCommonCodeUse("CARRIER_LOT_MANAGER_AUTH"))
            {
                CMM_ELEC_AREA_CODE_AUTH authConfirm2 = new CMM_ELEC_AREA_CODE_AUTH();
                authConfirm2.FrameOperation = FrameOperation;

                if (authConfirm2 != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = "CARRIER_LOT_MANAGER_AUTH";
                    Parameters[1] = "AUTH_PWD";

                    C1WindowExtension.SetParameters(authConfirm2, Parameters);

                    authConfirm2.Closed += new EventHandler(OnCloseAuthConfirm);
                    this.Dispatcher.BeginInvoke(new Action(() => authConfirm2.ShowModal()));
                }
            }
            else
            {
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveMappingInfo();
                    }
                });
            }
        }

        // <summary>
        // LOT 이력저장 인증 팝업 닫기
        // </summary>
        // <param name = "sender" ></ param >
        // < param name="e"></param>
        private void OnCloseAuthConfirm_Mapping(object sender, EventArgs e)
        {
            CMM001.CMM_COM_AUTH_CONFIRM window = sender as CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _UserID = window.UserID;
                SaveMappingInfo();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }

        private void SaveMappingInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));

                foreach (DataRowView drv in dgMapping.ItemsSource)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = DataTableConverter.GetValue(drv, "LOTID") as string;
                    newRow["CSTID"] = DataTableConverter.GetValue(drv, "CSTID") as string;

                    if (IsOpenPopupAuthorityConfirmByArea())
                    {
                        if (LoginInfo.USERTYPE == "P")
                        {
                            newRow["USERID"] = _UserID;//LoginInfo.USERID;
                        }
                        else
                        {
                            newRow["USERID"] = LoginInfo.USERID;
                        }
                    }
                    else
                    {
                        newRow["USERID"] = LoginInfo.USERID;
                    }
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_UI", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            DataTable dtMapping = DataTableConverter.Convert(dgMapping.ItemsSource);
                            DataTable dt = new DataTable();
                            dt.Columns.Add("LOTID", typeof(string));
                            dt.Columns.Add("CSTID", typeof(string));
                            dt.Columns.Add("USERID", typeof(string));
                            dt.Columns.Add("SRCTYPE", typeof(string));

                            for (int i = 0; i < dtMapping.Rows.Count; i++)
                            {
                                DataRow dr = dt.NewRow();
                                dr["LOTID"] = dtMapping.Rows[i]["LOTID"].ToString();
                                dr["CSTID"] = dtMapping.Rows[i]["CSTID"].ToString();
                                if (IsOpenPopupAuthorityConfirmByArea())
                                {
                                    if (LoginInfo.USERTYPE == "P")
                                    {
                                        dr["USERID"] = _UserID;//LoginInfo.USERID;
                                    }
                                    else
                                    {
                                        dr["USERID"] = LoginInfo.USERID;
                                    }
                                }
                                else
                                {
                                    dr["USERID"] = LoginInfo.USERID;
                                }
                                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                dt.Rows.Add(dr);

                                DataTable dtExcept = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CSTID_USING_EXCEPTION_CST", "INDATA", "RSLTDT", dt);

                                if (dtExcept != null || dtExcept.Rows.Count > 0)
                                {
                                    if (dtExcept.Rows[0]["OUT_FLAG"].ToString() == "NG")
                                    {
                                        Util.MessageException(searchException);
                                        return;
                                    }
                                }

                                dt.Clear();
                            }

                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        txtCSTID.Clear();
                        txtLOTID.Clear();
                        txtLOTID.IsReadOnly = true;
                        Util.gridClear(dgMapping);

                        resultTable = null;
                        InitializeResultTable();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void TabMapping_GotFocus(object sender, RoutedEventArgs e)
        {
            TabMapping.GotFocus -= TabMapping_GotFocus;
            TabEmpty.GotFocus += TabEmpty_GotFocus;
            TabInterlock.GotFocus += TabInterlock_GotFocus;
            ClearAll(txtCSTID, txtLOTID, dgMapping);
        }

        //  [E20231031-000680] : 'CARRIER_LOT_MANAGER_AUTH' 동별 공통코드에 따라 인증 팝업 호출
        private bool IsAreaCommonCodeUse(string sCodeType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = "AUTH_PWD";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) {
                Util.MessageException(ex);
            }

            return false;
        }

        //  [E20231031-000680] : 'CARRIER_LOT_MANAGER_AUTH' 동별 공통코드에 따라 인증 팝업 호출
        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            CMM_ELEC_AREA_CODE_AUTH window = sender as CMM_ELEC_AREA_CODE_AUTH;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SaveMappingInfo();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }
        #endregion

        #region [연계해제]
        private void txtCSTID_Empty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (!CanAddToRow(resultTable, txtCSTID_Empty, "CSTID"))
                return;

            CheckCarrierInfo(txtCSTID_Empty, txtLOTID_Empty, TYPE.EMPTY);
        }

        private void btnSave_Empty_Click(object sender, RoutedEventArgs e)
        {
            if (resultTable.Rows.Count == 0 || dgEmpty.Rows.Count == 0)
            {
                Util.MessageValidation("MMD0002");
                return;
            }

            if (IsOpenPopupAuthorityConfirmByArea() && LoginInfo.USERTYPE == "P") // 공통코드에 등록된 동 기준 && 공정PC 인 경우 사용자 인증 팝업 호출
            {
                CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new CMM001.CMM_COM_AUTH_CONFIRM();
                authConfirm.FrameOperation = FrameOperation;
                if (authConfirm != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = _authorityGroup;
                    Parameters[1] = "lgchem.com";
                    C1WindowExtension.SetParameters(authConfirm, Parameters);
                    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Release);

                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(authConfirm);
                            authConfirm.BringToFront();
                            break;
                        }
                    }
                }
            }
            else
            {
                // 사용중인 카세트 입니다. 초기화 하시겠습니까?
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4890"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    try
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            SaveEmptyInfo();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
        }

        // <summary>
        // 매핑해제 팝업닫기
        // </summary>
        // <param name = "sender" ></ param >
        // < param name="e"></param>
        private void OnCloseAuthConfirm_Release(object sender, EventArgs e)
        {
            CMM001.CMM_COM_AUTH_CONFIRM window = sender as CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _UserID = window.UserID;
                SaveEmptyInfo();
            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }

        private void SaveEmptyInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));

                foreach (DataRowView drv in dgEmpty.ItemsSource)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["CSTID"] = DataTableConverter.GetValue(drv, "CSTID") as string;

                    if (IsOpenPopupAuthorityConfirmByArea())
                    {
                        if (LoginInfo.USERTYPE == "P")
                        {
                            newRow["USERID"] = _UserID;//LoginInfo.USERID;
                        }
                        else
                        {
                            newRow["USERID"] = LoginInfo.USERID;
                        }
                    }
                    else
                    {
                        newRow["USERID"] = LoginInfo.USERID;
                    }
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_EMPTY_UI", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        txtCSTID_Empty.Clear();
                        txtLOTID_Empty.Clear();
                        Util.gridClear(dgEmpty);

                        resultTable = null;
                        InitializeResultTable();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void TabEmpty_GotFocus(object sender, RoutedEventArgs e)
        {
            TabEmpty.GotFocus -= TabEmpty_GotFocus;
            TabMapping.GotFocus += TabMapping_GotFocus;
            TabInterlock.GotFocus += TabInterlock_GotFocus;
            ClearAll(txtCSTID_Empty, txtLOTID_Empty, dgEmpty);
        }
        #endregion

        #region 비정상 Interlock 해제 [C20211002-000028]
        private void TabInterlock_GotFocus(object sender, RoutedEventArgs e)
        {
            TabInterlock.GotFocus -= TabInterlock_GotFocus;
            TabEmpty.GotFocus += TabEmpty_GotFocus;
            TabMapping.GotFocus += TabMapping_GotFocus;
            ClearAll(txtCSTID2, txtLOTID2, dgInterlock);
        }

        private void txtCSTID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (!CanAddToRow(resultTable, txtCSTID2, "CSTID"))
                return;

            CheckCarrierInfo(txtCSTID2, null, TYPE.LOCK);
        }

        private void btnDelRowLock_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            resultTable.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, resultTable, this.FrameOperation);
        }

        private void btnSave2_Click(object sender, RoutedEventArgs e)
        {
            if (resultTable.Rows.Count == 0 || dgInterlock.Rows.Count == 0)
            {
                Util.MessageValidation("MMD0002");
                return;
            }

            if (IsOpenPopupAuthorityConfirmByArea() && LoginInfo.USERTYPE == "P")    //공통코드에 등록된 동 기준 && 공정PC 인 경우 사용자 인증 팝업 호출
            {
                CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new CMM001.CMM_COM_AUTH_CONFIRM();
                authConfirm.FrameOperation = FrameOperation;
                if (authConfirm != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = _authorityGroup;
                    Parameters[1] = "lgchem.com";
                    C1WindowExtension.SetParameters(authConfirm, Parameters);
                    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Mapping);

                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(authConfirm);
                            authConfirm.BringToFront();
                            break;
                        }
                    }
                }
            }
            else
            {
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        GetSaveInterlock("", "MAPPING_ERR_RELEASE_MANUAL", "N");
                    }
                });
            }
        }

        private void GetSaveInterlock(string CSTID, string Actid, string ErrFlag)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("OCCR_FLAG", typeof(string));
                inTable.Columns.Add("HIST_FLAG", typeof(string));

                //매핑 오류 해제
                if (ErrFlag == "N")
                {
                    isInterLock = true;

                    foreach (DataRowView drv in dgInterlock.ItemsSource)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CSTID"] = DataTableConverter.GetValue(drv, "CSTID") as string;
                        if (IsOpenPopupAuthorityConfirmByArea())
                        {
                            if (LoginInfo.USERTYPE == "P")
                            {
                                newRow["USERID"] = _UserID;//LoginInfo.USERID;
                            }
                            else
                            {
                                newRow["USERID"] = LoginInfo.USERID;
                            }
                        }
                        else
                        {
                            newRow["USERID"] = LoginInfo.USERID;
                        }
                        newRow["ACTID"] = Actid;
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["OCCR_FLAG"] = ErrFlag;
                        newRow["HIST_FLAG"] = "Y";
                        inTable.Rows.Add(newRow);
                    }
                }
                //매핑 오류 설정
                else
                {
                    isInterLock = false;

                    DataRow newRow = inTable.NewRow();
                    newRow["CSTID"] = CSTID;
                    if (IsOpenPopupAuthorityConfirmByArea())
                    {
                        if (LoginInfo.USERTYPE == "P")
                        {
                            newRow["USERID"] = _UserID;//LoginInfo.USERID;
                        }
                        else
                        {
                            newRow["USERID"] = LoginInfo.USERID;
                        }
                    }
                    else
                    {
                        newRow["USERID"] = LoginInfo.USERID;
                    }
                    newRow["ACTID"] = Actid;
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["OCCR_FLAG"] = ErrFlag;
                    newRow["HIST_FLAG"] = "Y";
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_ERR_OCCR", "INDATA", null, inTable, (searchResult, ExMessge) =>
                {
                    try
                    {
                        if (ExMessge != null)
                        {
                            Util.MessageException(ExMessge);
                            return;
                        }

                        if (isInterLock)
                        {
                            Util.MessageInfo("SFU1275");//정상 처리 되었습니다.
                            Util.gridClear(dgInterlock);

                            resultTable = null;
                            InitializeResultTable();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
