/*************************************************************************************
 Created Date : 2017.01.14
      Creator : 
   Decription : 근무자그룹-근무자 Mapping
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_061 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string selectedWrkGr = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_061()
        {
            InitializeComponent();
            Initialize();
            InitGrid();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
        }

        private void Initialize()
        {
            InitCombo();
            InitGrid();
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgMappingUserList);
            Util.gridClear(dgNWRKList);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };


            // 활성화 시스템 일 때,
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT_FORM");
            }
            // 조립 시스템 일 때,
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            }


            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);

            //워크그룹
            Set_Combo_WRK_GR(cboWrkGr);
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchWRKGRData();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            deleteGRUserData();
        }

        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserName.Text))
            {
                Util.Alert("SFU1278"); //조회할 근무자명을 입력하세요.
                txtUserName.Focus();
                return;
            }
            searchNotGRUserData();
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            registerGRUserData();
        }

        private void txtProcess_KeyDown(object sender, KeyEventArgs e)
        {
            Set_Combo_WRK_GR(cboWrkGr);
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtUserName.Text))
                {
                    Util.Alert("SFU1278"); //조회할 근무자명을 입력하세요.
                    txtUserName.Focus();
                    return;
                }
                searchNotGRUserData();
            }
        }

        private void dgList_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            if (dgList.ItemsSource == null || dgList.Rows.Count == 0)
                return;
            if (dgList.CurrentRow == null || dgList.CurrentRow.Index < 0 || dgList.SelectedIndex < 0)
                return;
            if (dgList.CurrentRow.DataItem == null)
                return;

            int Select = dgList.SelectedIndex;
            selectedWrkGr = (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "WRK_GR_ID") ?? String.Empty).ToString();
            Util.gridClear(dgMappingUserList);
            Util.gridClear(dgNWRKList);
            searchGRUserData();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SearchWRKGRData();
            Set_Combo_WRK_GR(cboWrkGr);
        }
        #endregion

        #region Mehod
        private void Set_Combo_WRK_GR(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                drnewrow["PROCID"] = Util.GetCondition(cboProcess);
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_WRK_GR_CBO_BY_PROCESS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void SearchWRKGRData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                InitGrid();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                if (!Util.NVC(cboWrkGr.SelectedValue).Equals(""))
                {
                    IndataTable.Columns.Add("WRK_GR_ID", typeof(string));
                }

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = Util.GetCondition(cboArea);
                Indata["PROCID"] = Util.GetCondition(cboProcess);




                if (!Util.NVC(cboWrkGr.SelectedValue).Equals(""))
                {
                    Indata["WRK_GR_ID"] = Util.NVC(cboWrkGr.SelectedValue);
                }
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_AREA_PROC_GR", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgList, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void searchGRUserData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgMappingUserList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WRK_GR_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = Util.GetCondition(cboArea);
                Indata["PROCID"] = Util.GetCondition(cboProcess);
                Indata["WRK_GR_ID"] = selectedWrkGr;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_AREA_PROC_GR_USER_DETL", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgMappingUserList, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void searchNotGRUserData()
        {
            try
            {
                if (dgList.CurrentRow == null || dgList.CurrentRow.Index < 0 || dgList.SelectedIndex < 0)
                {
                    Util.Alert("SFU2049");  //선택된 근무자그룹이 없습니다.
                    txtUserName.Focus();
                    return;
                }
                if (dgList.CurrentRow.DataItem == null)
                {
                    Util.Alert("SFU2049");  //선택된 근무자그룹이 없습니다.
                    txtUserName.Focus();
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgNWRKList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WRK_GR_ID", typeof(string));
                if (!Util.NVC(txtUserName.Text).Equals(""))
                {
                    IndataTable.Columns.Add("USERNAME", typeof(string));
                    IndataTable.Columns.Add("USERID", typeof(string));
                }
           
                IndataTable.Columns.Add("ALL_CHK", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = Util.GetCondition(cboArea);
                Indata["PROCID"] = Util.GetCondition(cboProcess);
                Indata["WRK_GR_ID"] = selectedWrkGr;
                if (!Util.NVC(txtUserName.Text).Equals(""))
                {
                    Indata["USERNAME"] = Util.NVC(txtUserName.Text);
                    Indata["USERID"] = Util.NVC(txtUserName.Text);
                }
                Indata["ALL_CHK"] = chkAll.IsChecked;  //ALL 조건이 False이면 areaid 조건으로 검색 / True이면 전체 검색
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_AREA_PROC_USER_NOT_GR", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgNWRKList, result, FrameOperation, true);
                });
                txtUserName.Text = string.Empty;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void registerGRUserData()
        {
            try
            {
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable IndataTable = inDataSet.Tables.Add("INDATA");
                        IndataTable.Columns.Add("SRCTYPE", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));
                        IndataTable.Columns.Add("AREAID", typeof(string));
                        IndataTable.Columns.Add("PROCID", typeof(string));
                        IndataTable.Columns.Add("WRK_GR_ID", typeof(string));

                        DataRow Indata = IndataTable.NewRow();
                        Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        Indata["USERID"] = LoginInfo.USERID;
                        Indata["AREAID"] = Util.GetCondition(cboArea);
                        Indata["PROCID"] = Util.GetCondition(cboProcess);
                        Indata["WRK_GR_ID"] = selectedWrkGr;
                        IndataTable.Rows.Add(Indata);

                        DataTable InAddDataTable = inDataSet.Tables.Add("IN_INPUT");
                        InAddDataTable.Columns.Add("USERID", typeof(string));

                        for (int _iRow = 0; _iRow < dgNWRKList.Rows.Count; _iRow++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgNWRKList.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow InUserData = InAddDataTable.NewRow();
                                InUserData["USERID"] = Util.NVC(DataTableConverter.GetValue(dgNWRKList.Rows[_iRow].DataItem, "USERID"));
                                InAddDataTable.Rows.Add(InUserData);
                            }
                        }
                        if (IndataTable.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService_Multi("BR_BAS_REG_TB_MMD_AREA_PROC_GR_USER", "INDATA,IN_INPUT", null, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                    return;
                                }

                                Util.AlertInfo("SFU1270"); //저장되었습니다.
                                searchGRUserData();
                                Util.gridClear(dgNWRKList);
                            }, inDataSet);
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void deleteGRUserData()
        {
            try
            {
                //삭제하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable IndataTable = inDataSet.Tables.Add("INDATA");
                        IndataTable.Columns.Add("SRCTYPE", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));
                        IndataTable.Columns.Add("AREAID", typeof(string));
                        IndataTable.Columns.Add("PROCID", typeof(string));
                        IndataTable.Columns.Add("WRK_GR_ID", typeof(string));

                        DataRow Indata = IndataTable.NewRow();
                        Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        Indata["USERID"] = LoginInfo.USERID;
                        Indata["AREAID"] = Util.GetCondition(cboArea);
                        Indata["PROCID"] = Util.GetCondition(cboProcess);
                        Indata["WRK_GR_ID"] = selectedWrkGr;
                        IndataTable.Rows.Add(Indata);

                        DataTable InAddDataTable = inDataSet.Tables.Add("IN_INPUT");
                        InAddDataTable.Columns.Add("USERID", typeof(string));

                        for (int _iRow = 0; _iRow < dgMappingUserList.Rows.Count; _iRow++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgMappingUserList.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow InUserData = InAddDataTable.NewRow();
                                InUserData["USERID"] = Util.NVC(DataTableConverter.GetValue(dgMappingUserList.Rows[_iRow].DataItem, "USERID"));
                                InAddDataTable.Rows.Add(InUserData);
                            }
                        }
                        if (IndataTable.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService_Multi("BR_BAS_REG_CLOSE_AREA_PROC_GR_USER", "INDATA,IN_INPUT", null, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                    return;
                                }

                                Util.AlertInfo("SFU1273"); //삭제되었습니다.
                                searchGRUserData();
                                Util.gridClear(dgNWRKList);
                            }, inDataSet);
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        #endregion
    }
}
