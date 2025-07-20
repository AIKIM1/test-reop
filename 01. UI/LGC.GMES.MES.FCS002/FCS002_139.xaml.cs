/*************************************************************************************
 Created Date : 2022.09.01
      Creator : Kang Dong Hee
   Decription : Tray-Lot 정보관리(공Tray 처리)
--------------------------------------------------------------------------------------
 [Change History]
  2021.09.01  NAME   : Initial Created
  2021.09.01  강동희 : Tray 상태 체크 알람팝업 제거 및 화면상에 Validation 처리 구현

**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_139 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private DataTable dtTemp = new DataTable();
        private DataTable resultTable;
        private DataRow tmpDataRow;
        Util _Util = new Util();

        private string _UserID = string.Empty; //직접 실행하는 USerID
        private string _authorityGroup = string.Empty;
        #endregion

        #region [Initialize]
        public FCS002_139()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeResultTable();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitializeResultTable()
        {
            if (resultTable == null)
            {
                resultTable = new DataTable();
                resultTable.Columns.Add("CHK", typeof(bool));
                resultTable.Columns.Add("CSTTYPE", typeof(string));
                resultTable.Columns.Add("CSTID", typeof(string));
                resultTable.Columns.Add("CSTSTAT", typeof(string));
                resultTable.Columns.Add("LOTID", typeof(string));
                resultTable.Columns.Add("LOTTYPE", typeof(string));
                resultTable.Columns.Add("EMPTY_ENABLE_FLAG", typeof(string));
                resultTable.Columns.Add("INSUSER", typeof(string));
                resultTable.Columns.Add("INSDTTM", typeof(DateTime));
                resultTable.Columns.Add("UPDUSER", typeof(string));
                resultTable.Columns.Add("UPDDTTM", typeof(DateTime));
                resultTable.Columns.Add("CSTTNAME", typeof(string));
                resultTable.Columns.Add("CSTSNAME", typeof(string));
                resultTable.Columns.Add("PORTNAME", typeof(string));
            }
        }

        #endregion

        #region [Method]
        private void GetDetailInfo(string LOTID)
        {
            try
            {
                Util.gridClear(dgSubLotList);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(LOTID);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_WIP_FCS_RN_CELL_DETAIL_DRB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgSubLotList, result, FrameOperation, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool IsAreaCommoncodeAttrUse(string sCodeType, string sCodeName, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = 'Y';
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID, string sFormID)
        {
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = sFormID;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_FORM", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                if (bRet == false)
                {
                    string objectmessage = string.Empty;

                    if (sBtnGrpID == "TRAY_LOT_EMPTY_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("연계해제");

                    Util.MessageValidation("SFU3520", LoginInfo.USERID, objectmessage);     // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
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

        private void CheckCarrierInfo(TextBox _txtCstID, TextBox _txtLotID)
        {
            try
            {
                ShowLoadingIndicator();
                Util.gridClear(dgSubLotList);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
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
                            MessageValidationWithCallBack("FM_ME_0342", (result) =>
                            {
                                _txtCstID.Focus();
                            }, parameters); //CSTID[%1]에 해당하는 CST가 없습니다. SFU7001

                            return;
                        }

                        //에러 없는 경우
                        tmpDataRow = null;
                        tmpDataRow = resultTable.NewRow();
                        tmpDataRow["CSTTYPE"] = Util.NVC(searchResult.Rows[0]["CSTTYPE"]);
                        tmpDataRow["CSTID"] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                        tmpDataRow["CSTSTAT"] = Util.NVC(searchResult.Rows[0]["CSTSTAT"]);
                        tmpDataRow["CSTTNAME"] = Util.NVC(searchResult.Rows[0]["CSTTNAME"]);
                        tmpDataRow["CSTSNAME"] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);

                        _txtLotID.Text = Util.NVC(searchResult.Rows[0]["CURR_LOTID"]);

                        if (!string.IsNullOrEmpty(Util.NVC(_txtLotID.Text)))
                        {
                            CheckLotInfo(_txtCstID, _txtLotID);
                        }
                        else
                        {
                            tmpDataRow["EMPTY_ENABLE_FLAG"] = "N";
                            resultTable.Rows.Add(tmpDataRow);

                            _txtCstID.Clear();
                            _txtCstID.Focusable = true;
                            _txtCstID.Focus();

                            Util.GridSetData(dgTrayLotList, resultTable, this.FrameOperation);
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
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CheckLotInfo(TextBox _txtCstID, TextBox _txtLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _txtLotID.Text.Trim();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_SEL_VW_WIP_FORM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                                _txtLotID.Clear();

                                _txtCstID.Clear();
                                _txtCstID.Focusable = true;
                                _txtCstID.Focus();
                            }, parameters); //LOTID[%1]에 해당하는 LOT이 없습니다.

                            return;
                        }

                        //에러가 없는 경우
                        tmpDataRow["CHK"] = false;
                        tmpDataRow["LOTID"] = Util.NVC(searchResult.Rows[0]["LOTID"]);
                        tmpDataRow["LOTTYPE"] = Util.NVC(searchResult.Rows[0]["LOTTYPE"]);
                        string sLotDetlTypeCode = Util.NVC(searchResult.Rows[0]["LOT_DETL_TYPE_CODE"]);
                        string[] sAttrbute = { "Y" };
                        if (!IsAreaCommoncodeAttrUse("TRAY_LOT_EMPTY_LOT_DETL_TYPE_CODE", sLotDetlTypeCode, sAttrbute))
                        {
                            tmpDataRow["EMPTY_ENABLE_FLAG"] = "N";
                        }
                        else
                        {
                            tmpDataRow["EMPTY_ENABLE_FLAG"] = "Y";
                        }
                        tmpDataRow["INSUSER"] = LoginInfo.USERID;
                        tmpDataRow["INSDTTM"] = DateTime.Now;
                        tmpDataRow["UPDUSER"] = LoginInfo.USERID;
                        tmpDataRow["UPDDTTM"] = tmpDataRow["INSDTTM"];

                        resultTable.Rows.Add(tmpDataRow);

                        _txtCstID.Clear();
                        _txtCstID.Focusable = true;
                        _txtCstID.Focus();

                        Util.GridSetData(dgTrayLotList, resultTable, this.FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]
        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtTrayID.Text)) && (e.Key == Key.Enter))
            {

                if (!CanAddToRow(resultTable, txtTrayID, "CSTID"))
                    return;

                CheckCarrierInfo(txtTrayID, txtTrayLotID);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!FrameOperation.AUTHORITY.Equals("W"))
            {
                return;
            }

            // 1. 해당 동 적용 여부 체크
            // 2. 적용 동 버튼 적용 여부 체크
            string[] sAttrbute = { null, "FCS002_139" };

            if (IsAreaCommoncodeAttrUse("PERMISSIONS_PER_BUTTON_FORM", "", sAttrbute))
                if (!CheckButtonPermissionGroupByBtnGroupID("TRAY_LOT_EMPTY_W", "FCS002_139")) return;

            if (resultTable.Rows.Count == 0 || dgTrayLotList.Rows.Count == 0)
            {
                Util.MessageValidation("MMD0002");
                return;
            }

            foreach (DataRowView drv in dgTrayLotList.ItemsSource)
            {
                string sLotDetlTypeCode = Util.NVC(DataTableConverter.GetValue(drv, "EMPTY_ENABLE_FLAG") as string);
                string sTrayID = Util.NVC(DataTableConverter.GetValue(drv, "CSTID") as string);
                if (!sLotDetlTypeCode.Equals("Y"))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(sTrayID);
                    MessageValidationWithCallBack("FM_ME_0451", (result) =>
                    {
                    }, parameters); //Tray [%1]은 공 Tray 처리 할 수 없는 Tray 입니다.

                    return;
                }

            }

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

                foreach (DataRowView drv in dgTrayLotList.ItemsSource)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["CSTID"] = DataTableConverter.GetValue(drv, "CSTID") as string;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_EMPTY_UI_FORM", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        txtTrayID.Clear();
                        txtTrayLotID.Clear();
                        Util.gridClear(dgTrayLotList);
                        Util.gridClear(dgSubLotList);

                        resultTable = null;
                        InitializeResultTable();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void dgTrayLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTrayLotList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == null)
                    {
                        return;
                    }

                    if (cell.Column.Name != null & cell.Column.Name.Equals("LOTID"))
                    {
                        string sLotID = Util.NVC(DataTableConverter.GetValue(dgTrayLotList.CurrentRow.DataItem, "LOTID"));
                        GetDetailInfo(sLotID);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void dgTrayLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    string sEmptyUseFlag = Util.NVC(DataTableConverter.GetValue(dgTrayLotList.Rows[e.Cell.Row.Index].DataItem, "EMPTY_ENABLE_FLAG"));

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "LOTID")
                    {
                        if (sEmptyUseFlag.Equals("Y"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "EMPTY_ENABLE_FLAG")
                    {
                        if (!sEmptyUseFlag.Equals("Y"))
                        {
                            e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }
                        else
                        {
                            e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }

                }
            }));
        }

        private void dgTrayLotList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgSubLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////
                }
            }));
        }

        private void dgSubLotList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void btnDelRow_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            resultTable.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, resultTable, this.FrameOperation);

            if (resultTable.Rows.Count == 0)
            {
                Util.gridClear(dgSubLotList);
                txtTrayLotID.Text = string.Empty;
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
        #endregion

    }
}
