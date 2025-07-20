/*************************************************************************************
 Created Date : 2020.
      Creator : 
   Decription : Tray Cell 정보변경
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.
  2023.01.31  조영대 : Validation 수정
**************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_035 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        private string sCell = null;
        DataTable dtCellList = new DataTable();
        public FCS001_035()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //  InitCombo();
            //  InitControl();
            //  SetEvent();
        }

        #endregion

        #region [Method]
        private void GetCellList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = Util.GetCondition(txtTrayID, sMsg: "FM_ME_0070");  //Tray ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["CSTID"].ToString()))
                    return;
                dtRqst.Rows.Add(dr);

                dtCellList = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_ID_LIST", "RQSTDT", "RSLTDT", dtRqst);
                if (dtCellList.Rows.Count > 0)
                {
                    dtCellList.Columns.Add("CHK", typeof(bool));
                    Util.GridSetData(dgTrayList, dtCellList, this.FrameOperation);
                }

                else
                {
                    Util.MessageInfo("FM_ME_0217");  //정보 변경 가능한 Tray가 아닙니다.
                }
            }
            catch (Exception ex) { }
        }
        #endregion

        #region [Event]
        private void btnTraySearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ClearValidation();
                if (txtTrayID.GetBindValue() == null)
                {
                    txtTrayID.SetValidation(MessageDic.Instance.GetMessage("SFU4975"));
                    return;
                }

                if (dgTrayList.Rows.Count > 0 || dgDummyCellList.Rows.Count > 0)
                {
                    Util.MessageConfirm("FM_ME_0203", result => //작업중이던 정보가 모두 초기화 됩니다. 계속 진행 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Util.gridClear(dgDummyCellList);
                            Util.gridClear(dgTrayList);
                            GetCellList();
                        }
                    });
                }
                else GetCellList();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ClearValidation();

                if (dgTrayList.Rows.Count == 0)
                {
                    // 조회된 데이터가 없습니다.
                    Util.MessageValidation("SFU3537");
                    return;
                }

                Util.MessageConfirm("FM_ME_0204", result =>//작업하신 Tray 정보를 저장하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataSet dsIndata = new DataSet();
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "INDATA";
                            dtRqst.Columns.Add("SRCTYPE", typeof(string));
                            dtRqst.Columns.Add("IFMODE", typeof(string));
                            dtRqst.Columns.Add("AREAID", typeof(string));
                            dtRqst.Columns.Add("CSTID", typeof(string));
                            dtRqst.Columns.Add("CSTSLOT", typeof(string));
                            dtRqst.Columns.Add("SUBLOTID", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));
                            dtRqst.Columns.Add("ADD_SUBLOT_FLAG", typeof(string));

                            for (int i = 0; i < dgTrayList.Rows.Count; i++)
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                dr["CSTID"] = DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID");
                                dr["CSTSLOT"] = i + 1;
                                dr["SUBLOTID"] = DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "SUBLOTID");
                                dr["USERID"] = LoginInfo.USERID;
                                bool flag = false; // 기존 : true, 신규 : false
                                for (int j = 0; j < dtCellList.Rows.Count; j++)
                                {
                                    if (dtCellList.Rows[j]["SUBLOTID"].Equals(dr["SUBLOTID"]))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag) dr["ADD_SUBLOT_FLAG"] = "Y";

                                dtRqst.Rows.Add(dr);
                            }
                            dsIndata.Tables.Add(dtRqst);


                            DataSet dsRet = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_CHANGE_TRAY_CELL_INFO", "INDATA", "OUTDATA,CELL_DATA", dsIndata);

                            if (dsRet != null)
                            {
                                if (dsRet.Tables["OUTDATA"].Rows.Count == 0 || dsRet.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("1"))
                                {
                                    Util.MessageInfo("FM_ME_0073");  //Tray 정보 변경에 실패하였습니다.
                                    return;
                                }
                                else
                                {
                                    dsRet.Tables["CELL_DATA"].Columns.Add("CHK", typeof(bool));
                                    Util.GridSetData(dgTrayList, dsRet.Tables["CELL_DATA"], this.FrameOperation);
                                    Util.gridClear(dgDummyCellList);
                                    Util.MessageInfo("FM_ME_0074");  //Tray 정보 변경을 완료하였습니다.
                                }
                            }
                            else
                            {
                                Util.MessageInfo("FM_ME_0073");  //Tray 정보 변경에 실패하였습니다.
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                    }

                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int nDummyCell = -1;
                string sCellID = string.Empty;
                if (dgDummyCellList.Rows.Count > 0)
                {
                    for (int i = 0; i < dgDummyCellList.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE")
                            || Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "CHK")).Equals("1"))
                        {
                            sCellID = Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "SUBLOTID"));
                            nDummyCell = i;
                        }
                    }
                }
                else return;

                bool bSelect = false;
                int nEmptySlot = -1;
                if (dgTrayList.Rows.Count > 0)
                {
                    for (int j = 0; j < dgTrayList.Rows.Count; j++)
                    {
                        if ((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "CHK")).ToUpper().Equals("TRUE")
                            || Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "CHK")).Equals("1"))
                            && Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "SUBLOTID")).Equals("0000000000"))
                        {
                            bSelect = true;
                            nEmptySlot = j;
                        }
                    }
                }
                if (!bSelect)
                {
                    Util.MessageInfo("FM_ME_0151");  //비어있는 Slot을 선택해주세요.
                    return;
                }
                if (nEmptySlot != -1)
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("CSTID", typeof(string));
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[nEmptySlot].DataItem, "CSTID"));
                    dr["SUBLOTID"] = sCellID;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PRODUCT_BY_TRAY_ID", "RQSTDT", "RSLTDT", dtRqst);

                    bool flag = false; // 기존재 : true, 신규 : false
                    for (int i = 0; i < dtCellList.Rows.Count; i++)
                    {
                        if (dr["SUBLOTID"].Equals(dtCellList.Rows[i]["SUBLOTID"]))
                        { flag = true; break; }
                    }

                    if (dtRslt.Rows.Count == 0 && !flag)
                    {
                        Util.MessageInfo("FM_ME_0022");  //Cell 제품코드를 확인해주세요.
                        return;
                    }
                    // DataTableConverter.SetValue(dgTrayList.Rows[nEmptySlot].DataItem, "SUBLOTID", sCellID);

                    DataTable temp = DataTableConverter.Convert(dgTrayList.ItemsSource);
                    temp.Rows[nEmptySlot]["SUBLOTID"] = sCellID;
                    Util.GridSetData(dgTrayList, temp, this.FrameOperation);

                    temp = DataTableConverter.Convert(dgDummyCellList.ItemsSource);
                    temp.Rows.RemoveAt(nDummyCell);
                    Util.GridSetData(dgDummyCellList, temp, this.FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnCellMoveToDummy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet dsIndata = new DataSet();
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                // dtRqst.Columns.Add("FIN_CD", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));

                ArrayList sExistCell = new ArrayList();
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK") != null
                         && (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE")
                         || Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1")))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "SUBLOTID"));
                        dr["WIPSTAT"] = "PROC|WAIT";
                        dtRqst.Rows.Add(dr);
                    }
                }
                if (dtRqst.Rows.Count == 0)
                {
                    Util.MessageInfo("FM_ME_0161");  //선택된 Cell ID가 없습니다.
                    return;
                }
                for (int i = 0; i < dgDummyCellList.Rows.Count; i++)
                {
                    DataRow[] drSelectList = dtRqst.Select(string.Format("SUBLOTID = '{0}'", Util.NVC(dgDummyCellList.Rows[i].DataItem, "SUBLOTID")));

                    foreach (DataRow drSelect in drSelectList)
                    {
                        sExistCell.Add(drSelect["SUBLOTID"].ToString());
                    }

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "SUBLOTID"));
                    dr["WIPSTAT"] = Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "WIPSTAT"));

                    dtRqst.Rows.Add(dr);
                }

                dsIndata.Tables.Add(dtRqst);

                if (sExistCell.Count > 0)
                {
                    string sDupCell = string.Empty;
                    foreach (string s in sExistCell)
                    {
                        sDupCell += s + ", ";
                    }
                    sDupCell.TrimEnd(", ".ToCharArray());

                    //Util.ME("ME_0001", new string[] { sDupCell });  //[Cell ID : {0}]는 Dummy Cell 목록에 존재합니다.
                    return;
                }

                DataSet dsRet = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CELL_INFO_BY_FINCD", "RQSTDT", "OUTDATA,CELL_INFO", dsIndata);


                if (dsRet != null)
                {
                    if (dsRet.Tables["OUTDATA"].Rows.Count == 0 || dsRet.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() != "0")
                    {
                        Util.MessageInfo("FM_ME_0021");  //Cell 정보가 존재하지않습니다.
                        return;
                    }
                    dsRet.Tables["CELL_INFO"].Columns.Add("CHK", typeof(bool));

                    Util.GridSetData(dgDummyCellList, dsRet.Tables["CELL_INFO"], this.FrameOperation);
                    for (int i = 0; i < dgTrayList.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK") != null
                        && (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE")
                        || Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1")))
                        {
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "SUBLOTID", "0000000000");
                        }
                    }
                    DataTable temp = DataTableConverter.Convert(dgTrayList.ItemsSource);
                    Util.gridClear(dgTrayList);
                    Util.GridSetData(dgTrayList, temp, this.FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnCellAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ClearValidation();
                if (txtDummyCellID.GetBindValue() == null)
                {
                    txtDummyCellID.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0019"));
                    return;
                }

                DataSet dsIndata = new DataSet();
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = Util.GetCondition(txtDummyCellID, sMsg: "FM_ME_0019");  //Cell ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;

                dtRqst.Rows.Add(dr);
                dsIndata.Tables.Add(dtRqst);
                DataSet dsRet = new ClientProxy().ExecuteServiceSync_Multi("DA_SEL_CELL_INFO_ADD_CELL", "RQSTDT", "CELL_INFO", dsIndata);

                if (dsRet != null)
                {
                    if (dsRet.Tables["CELL_INFO"].Rows.Count == 0)
                    {
                        Util.MessageInfo("FM_ME_0021");  //Cell 정보가 존재하지않습니다.
                        return;
                    }
                }
                else
                    return;

                DataTable dtCellInfo = new DataTable();

                dtCellInfo.TableName = "RQSTDT";
                dtCellInfo.Columns.Add("SUBLOTID", typeof(string));
                dtCellInfo.Columns.Add("WIPSTAT", typeof(string));

                DataRow drAddCell = dtCellInfo.NewRow();
                drAddCell["SUBLOTID"] = dsRet.Tables["CELL_INFO"].Rows[0]["SUBLOTID"].ToString();
                drAddCell["WIPSTAT"] = dsRet.Tables["CELL_INFO"].Rows[0]["WIPSTAT"].ToString();
                dtCellInfo.Rows.Add(drAddCell);

                for (int i = 0; i < dgDummyCellList.Rows.Count; i++)
                {
                    if (txtDummyCellID.Text.Equals(Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "SUBLOTID"))))
                    {
                        Util.AlertInfo("FM_ME_0001", new object[]
                        { Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "SUBLOTID")) }); //[Cell ID : {0}]는 Dummy Cell 목록에 존재합니다.
                        return;
                    }

                    DataRow drExist = dtCellInfo.NewRow();
                    drExist["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "SUBLOTID"));
                    drExist["WIPSTAT"] = Util.NVC(DataTableConverter.GetValue(dgDummyCellList.Rows[i].DataItem, "WIPSTAT"));
                    dtCellInfo.Rows.Add(drExist);
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_INFO_TERM", "RQSTDT", "RSLTDT", dtCellInfo);

                if (dtRslt != null)
                {
                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.MessageInfo("FM_ME_0021");  //Cell 정보가 존재하지않습니다.
                        return;
                    }
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    Util.GridSetData(dgDummyCellList, dtRslt, this.FrameOperation);
                    txtDummyCellID.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Column.Name.Equals("SUBLOTID"))
                {
                    string s = Util.NVC(e.Cell.Text);
                    if (string.IsNullOrEmpty(s) || s.ToUpper().Equals("0000000000"))
                    {
                        e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                    }
                    else if (!string.IsNullOrEmpty(sCell))
                    {
                        if (sCell.Equals(e.Cell.Text))
                        {
                            e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                        }
                    }
                    else
                    {
                        dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["CHK"].Index).Presenter.IsEnabled = true;
                    }
                }
            }));

        }

        private void btnSearchCell_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            this.ClearValidation();
            if (txtCellID.GetBindValue() == null)
            {
                txtCellID.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0019"));
                return;
            }

            sCell = txtCellID.Text;
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", false);
            }
            DataTable temp = DataTableConverter.Convert(dgTrayList.ItemsSource);
            Util.gridClear(dgTrayList);
            Util.GridSetData(dgTrayList, temp, this.FrameOperation);
        }

        private void dgTrayList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            // 체크박스 단일 선택
            if (sender == null || e.Cell == null) return;

            if (e.Cell.Column.Index == 0)
            {
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (e.Cell.Row.Index != i)
                        DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void dgDummyCellList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell == null) return;

            if (e.Cell.Column.Index == 0)
            {
                for (int i = 0; i < dgDummyCellList.Rows.Count; i++)
                {
                    if (e.Cell.Row.Index != i)
                        DataTableConverter.SetValue(dgDummyCellList.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        /*
private void fpTrayCellID_CellClick(object sender, FarPoint.Win.Spread.CellClickEventArgs e)
{
try
{
if (fpsTrayCellID.ActiveSheet.RowCount > 0 && e.Column == 0)
{
for (int i = 0; i < fpsTrayCellID.ActiveSheet.RowCount; i++)
{
fpsTrayCellID.ActiveSheet.Cells[i, 0].Value = false;
fpsTrayCellID.ActiveSheet.Rows[i].BackColor = Color.Empty;
}
}
}
catch (Exception ex)
{
Util.ExceptionMsg(ex);
}
}
*/
        #endregion
    }
}
