/*************************************************************************************
 Created Date : 2021.11.09
      Creator : 공민경
   Decription : Carrier -Lot 연계, 연계해제 관리(ESNJ)
--------------------------------------------------------------------------------------
 [Change History]
  2021.11.09  공민경 : Initial Created.   
  2024.04.16  성민식 : E20240328-001416 - CST Empty 상태여도 재공 - CST 매핑 해제 할 수 있도록 수정
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

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_366_CST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_366_CST : UserControl, IWorkArea
    {
        #region [공통]
        private DataTable resultTable;
        private DataRow tmpDataRow;

        private DataTable resultTable_Empty;
        private DataRow tmpDataRow_Empty;

        private string _UserID = string.Empty; //직접 실행하는 USerID
        private string _authorityGroup = string.Empty;

        private enum TYPE
        {
            MAPPING = 0,
            EMPTY = 1
        }

        public COM001_366_CST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
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
            }

            if (resultTable_Empty == null)
            {
                resultTable_Empty = new DataTable();
                resultTable_Empty.Columns.Add("CHK", typeof(bool));
                resultTable_Empty.Columns.Add("AREANAME", typeof(string));
                resultTable_Empty.Columns.Add("AREAID", typeof(string));
                resultTable_Empty.Columns.Add("CSTTYPE", typeof(string));
                resultTable_Empty.Columns.Add("CSTID", typeof(string));
                resultTable_Empty.Columns.Add("CSTSTAT", typeof(string));
                resultTable_Empty.Columns.Add("LOTID", typeof(string));
                resultTable_Empty.Columns.Add("INSUSER", typeof(string));
                resultTable_Empty.Columns.Add("INSDTTM", typeof(DateTime));
                resultTable_Empty.Columns.Add("UPDUSER", typeof(string));
                resultTable_Empty.Columns.Add("UPDDTTM", typeof(DateTime));
                resultTable_Empty.Columns.Add("CSTTNAME", typeof(string));
                resultTable_Empty.Columns.Add("CSTSNAME", typeof(string));
            }
        }

        private void CheckCarrierInfo(TextBox _txtCstID, TextBox _txtLotID, TYPE _type)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTID"] = _txtCstID.Text.Trim();
                inTable.Rows.Add(newRow);

                DataTable inLotTable = new DataTable();
                inLotTable.Columns.Add("MCS_CST_ID", typeof(string));

                DataRow newLotRow = inLotTable.NewRow();
                newLotRow["MCS_CST_ID"] = _txtCstID.Text.Trim();
                inLotTable.Rows.Add(newLotRow);

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
                                //2024.04.16  성민식 : E20240328-001416 - CST Empty 상태여도 재공 - CST 매핑 해제 할 수 있도록 수정
                                //if (Util.NVC(searchResult.Rows[0]["CSTSTAT"]).Equals("E"))
                                //{
                                //    object[] parameters = new object[2];
                                //    parameters[0] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                                //    parameters[1] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);
                                //    MessageValidationWithCallBack("SFU7002", (result) =>
                                //    {
                                //        _txtCstID.Focus();

                                //        _txtLotID.Clear();
                                //        _txtLotID.IsReadOnly = true;
                                //    }, parameters); //CSTID[%1] 이 상태가 %2 입니다.

                                //    return;
                                //}
                                break;
                        }

                        //에러 없는 경우
                        if (_type == TYPE.MAPPING)
                        {
                            tmpDataRow = null;
                            tmpDataRow = resultTable.NewRow();
                            tmpDataRow["CSTTYPE"] = Util.NVC(searchResult.Rows[0]["CSTTYPE"]);
                            tmpDataRow["CSTID"] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                            tmpDataRow["CSTSTAT"] = Util.NVC(searchResult.Rows[0]["CSTSTAT"]);
                            tmpDataRow["CSTTNAME"] = Util.NVC(searchResult.Rows[0]["CSTTNAME"]);
                            tmpDataRow["CSTSNAME"] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);

                            _txtLotID.Clear();
                            _txtLotID.IsReadOnly = false;
                            _txtLotID.Focus();
                        }
                        else if (_type == TYPE.EMPTY)
                        {
                            tmpDataRow_Empty = null;
                            tmpDataRow_Empty = resultTable_Empty.NewRow();
                            tmpDataRow_Empty["CSTTYPE"] = Util.NVC(searchResult.Rows[0]["CSTTYPE"]);
                            tmpDataRow_Empty["CSTID"] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                            tmpDataRow_Empty["CSTSTAT"] = Util.NVC(searchResult.Rows[0]["CSTSTAT"]);
                            tmpDataRow_Empty["CSTTNAME"] = Util.NVC(searchResult.Rows[0]["CSTTNAME"]);
                            tmpDataRow_Empty["CSTSNAME"] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);

                            CheckLotInfo_EMPTY(_txtCstID, _txtLotID, TYPE.EMPTY);
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

        private void CheckLotInfo_MAPPING(TextBox _txtCstID, TextBox _txtLotID, TYPE _type)
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
                                _txtLotID.Focus();
                            }, parameters); //LOTID[%1]에 해당하는 LOT이 없습니다.

                            return;
                        }

                        //재공상태 - 이동중인 상태만 막음.
                        //C20200518-000490
                        if (Util.NVC(searchResult.Rows[0]["WIPSTAT"]).Equals("MOVING"))
                        {
                            Util.MessageValidation("SFU2063", (result) =>
                            {
                                _txtLotID.Clear();
                                _txtLotID.IsReadOnly = false;
                                _txtLotID.Focusable = true;
                                _txtLotID.Focus();
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
                        if (!string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CSTID"])))
                        {
                            //object[] parameters = new object[2];
                            //parameters[0] = Util.NVC(searchResult.Rows[0]["LOTID"]);
                            //parameters[1] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                            //MessageValidationWithCallBack("SFU7003", (result) =>
                            //{
                            //    _txtLotID.Focus();
                            //}, parameters); //LOTID[%1]에 연계된 CSTID[%2]가 있습니다.

                            //return;

                            // PANCAKE에 매핑된 SKID 정보를 CARRIER 테이블에서 조회하여 없을 경우 계속 진행하도록 수정
                            DataTable RQSTDT = new DataTable("RQSTDT");
                            RQSTDT.Columns.Add("CSTID", typeof(string));

                            DataRow dr = RQSTDT.NewRow();
                            dr["CSTID"] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                            RQSTDT.Rows.Add(dr);

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER", "RQSTDT", "RSLTDT", RQSTDT);

                            if (dtResult.Rows.Count > 0)
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
                        // MES 2.0 ItemArray 위치 오류 Patch
                        //resultTable.Rows.Add(tmpDataRow.ItemArray);
                        resultTable.AddDataRow(tmpDataRow);

                        DataTable CstTable = new DataTable();
                        CstTable.Columns.Add("CSTID", typeof(string));

                        for (int i = 0; i < resultTable.Rows.Count; i++)
                        {
                            CstTable.Rows.Add(resultTable.Rows[i]["CSTID"]);
                        }

                        DataTable DistinctCstTable = CstTable.DefaultView.ToTable(true);
                        decimal MaxCCnt = MaxCount();

                        for (int j = 0; j < DistinctCstTable.Rows.Count; j++)
                        {
                            decimal Cnt = 0;

                            for (int k = 0; k < resultTable.Rows.Count; k++)
                            {
                                string CSTID = resultTable.Rows[k]["CSTID"].ToString();
                                string DISTINCT_CSTID = DistinctCstTable.Rows[j]["CSTID"].ToString();

                                if (CSTID == DISTINCT_CSTID)
                                {
                                    Cnt = Cnt + 1;
                                }

                                if (Cnt > MaxCCnt)
                                {
                                    Util.MessageInfo("SFU5110"); //최대 적재 수량을 초과할 수 없습니다.

                                    resultTable.Rows.RemoveAt(k);

                                    _txtLotID.Clear();
                                    _txtCstID.Clear();
                                    _txtLotID.IsReadOnly = true;
                                    //_txtCstID.Focus();

                                    return;
                                }
                            }
                        }

                        _txtLotID.Clear();
                        Util.GridSetData(dgMapping, resultTable, this.FrameOperation);
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

        private void CheckLotInfo_EMPTY(TextBox _txtCstID, TextBox _txtLotID, TYPE _type)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTID"] = _txtCstID.Text.Trim();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_VW_WIP_BY_CSTID", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        for (int i = 0; i < searchResult.Rows.Count; i++)
                        {

                            //데이터 없는 경우
                            if (searchResult.Rows.Count == 0)
                            {
                                object[] parameters = new object[1];
                                parameters[0] = searchResult.Rows[i]["LOTID"].ToString();
                                MessageValidationWithCallBack("SFU7000", (result) =>
                                {
                                    _txtLotID.Clear();

                                    _txtCstID.Clear();
                                    _txtCstID.Focusable = true;
                                    _txtCstID.Focus();
                                }, parameters); //LOTID[%1]에 해당하는 LOT이 없습니다.

                                return;
                            }

                            //재공상태 - 이동중인 상태만 막음.
                            //C20200518-000490
                            if (Util.NVC(searchResult.Rows[i]["WIPSTAT"]).Equals("MOVING"))
                            {
                                Util.MessageValidation("SFU2063", (result) =>
                                {
                                    _txtLotID.Clear();

                                    _txtCstID.Clear();
                                    _txtCstID.Focusable = true;
                                    _txtCstID.Focus();
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

                            //에러가 없는 경우
                            tmpDataRow_Empty["CHK"] = false;
                            tmpDataRow_Empty["AREANAME"] = LoginInfo.CFG_AREA_NAME;
                            tmpDataRow_Empty["AREAID"] = LoginInfo.CFG_AREA_ID;
                            tmpDataRow_Empty["LOTID"] = Util.NVC(searchResult.Rows[i]["LOTID"]);
                            tmpDataRow_Empty["INSUSER"] = LoginInfo.USERID;
                            tmpDataRow_Empty["INSDTTM"] = DateTime.Now;
                            tmpDataRow_Empty["UPDUSER"] = LoginInfo.USERID;
                            tmpDataRow_Empty["UPDDTTM"] = tmpDataRow_Empty["INSDTTM"];
                            // MES 2.0 ItemArray 위치 오류 Patch
                            //resultTable_Empty.Rows.Add(tmpDataRow_Empty.ItemArray);
                            resultTable_Empty.AddDataRow(tmpDataRow_Empty);
                        }

                        _txtCstID.Clear();
                        _txtCstID.Focusable = true;
                        _txtCstID.Focus();

                        Util.GridSetData(dgEmpty, resultTable_Empty, this.FrameOperation);
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

        private decimal MaxCount()
        {
            decimal cnt = 0;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CMCODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "SKID_MAX_LOT";
            dr["CMCODE"] = "C";

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count > 0)
            {
                cnt = Convert.ToDecimal(dtResult.Rows[0]["ATTRIBUTE1"].ToString());
                return cnt;
            }

            return cnt;
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

        private void btnDelRowEmpty_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;

            string CSTID = resultTable_Empty.Rows[clickedIndex]["CSTID"].ToString();

            for(int i = resultTable_Empty.Rows.Count - 1; i >= 0; i--)
            {
                if(resultTable_Empty.Rows[i]["CSTID"].ToString() == CSTID)
                {
                    resultTable_Empty.Rows.RemoveAt(i);
                }
            }

            //resultTable_Empty.Rows.RemoveAt(clickedIndex);

            Util.GridSetData(presenter.DataGrid, resultTable_Empty, this.FrameOperation);
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
            resultTable_Empty.Clear();

            Util.gridClear(dg);
        }
        #endregion

        #region [연계처리]
        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            //if (!CanAddToRow(resultTable, txtCSTID, "CSTID"))
            //    return;

            CheckCarrierInfo(txtCSTID, txtLOTID, TYPE.MAPPING);
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (!CanAddToRow(resultTable, txtLOTID, "LOTID"))
                return;

            CheckLotInfo_MAPPING(txtCSTID, txtLOTID, TYPE.MAPPING);
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

                new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_UI_NJ", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
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
            //ClearAll(txtCSTID, txtLOTID, dgMapping);
        }
        #endregion

        #region [연계해제]
        private void txtCSTID_Empty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (!CanAddToRow(resultTable_Empty, txtCSTID_Empty, "CSTID"))
                return;

            CheckCarrierInfo(txtCSTID_Empty, txtLOTID_Empty, TYPE.EMPTY);
        }

        private void btnSave_Empty_Click(object sender, RoutedEventArgs e)
        {
            if (resultTable_Empty.Rows.Count == 0 || dgEmpty.Rows.Count == 0)
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

                DataTable inTable_Distinct = inTable.DefaultView.ToTable(true);

                new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_EMPTY_UI_NJ", "INDATA", null, inTable_Distinct, (searchResult, searchException) =>
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

                        resultTable_Empty = null;
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
            //ClearAll(txtCSTID_Empty, txtLOTID_Empty, dgEmpty);
        }
        #endregion
    }
}
