/*************************************************************************************
 Created Date : 2017.12.08
      Creator : 정규환C
   Decription : C20171201_46507 - [업무개선] GMES 작업조건등록 조회 창 개설 요청
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.28  INS 김동일K : Initial Created.
  
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using System.Linq;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_EQPT_COND_SEARCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_EQPT_COND_SEARCH : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _ProcID = string.Empty;
        private string _EqptID = string.Empty;

        private string _LineName = string.Empty;
        private string _EqptName = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        #endregion

        #region Initialize    
        public CMM_ASSY_EQPT_COND_SEARCH()
        {
            InitializeComponent();
        }
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            string[] tmpFilter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: tmpFilter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            //작업조
            C1ComboBox[] cboShiftParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.NONE, cbParent: cboShiftParent, sCase: "SHIFT", sFilter: tmpFilter);
            const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, _LineID, _ProcID };
            string selectedValueText = cboShift.SelectedValuePath;
            string displayMemberText = cboShift.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cboShift, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);

        }
        #endregion

        #region Event
        #endregion

        #region Mehod

        #region Biz

        private void GetEqptPrdtGetItem()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));         // 언어
                inDataTable.Columns.Add("PROCID", typeof(string));         // 공정
                inDataTable.Columns.Add("EQPTID", typeof(string));         // 설비
                inDataTable.Columns.Add("DATEFROM", typeof(string));       // 일자
                inDataTable.Columns.Add("DATETO", typeof(string));         // 일자

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["DATEFROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                newRow["DATETO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_PROC_EQPT_PRDT_GET_ITEM_INFO", "INDATA", "OUTDATA", inDataTable);

                dgEqptCond.MergingCells -= dgEqptCond_MergingCells;

                Util.GridSetData(dgEqptCond, dtRslt, FrameOperation, false);

                dgEqptCond.MergingCells += dgEqptCond_MergingCells;
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
        }
        #endregion

        #region Func
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearch);
            //listAuth.Add(btnLossDfctSave);
            //listAuth.Add(btnPrdChgDfctSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void InitializeGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                Util.gridClear(dg);

                if (dg == null) return;

                List<C1.WPF.DataGrid.DataGridColumn> dgList = new List<C1.WPF.DataGrid.DataGridColumn>();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    if (col.Name.IndexOf(":") >= 0)
                    {
                        dgList.Add(col);
                    }
                }

                if (dgList.Count > 0)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn col in dgList)
                    {
                        dg.Columns.Remove(col);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeGridHeaders()
        {
            try
            {
                if (dgEqptCond == null || dgEqptCond.ItemsSource == null) return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 5)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _ProcID = Util.NVC(tmps[1]);
                    _EqptID = Util.NVC(tmps[2]);
                    _LineName = Util.NVC(tmps[3]);
                    _EqptName = Util.NVC(tmps[4]);
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _ProcID = "";
                    _LineName = "";
                    _EqptName = "";
                }

                // 남경인 경우 화면 SIZE 조정..
                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                {
                    this.Width = 800;
                    this.Height = 700;
                }

                ApplyPermissions();

                // 라인
                if (cboEquipmentSegment != null && cboEquipmentSegment.Items != null && cboEquipmentSegment.Items.Count > 0)
                {
                    cboEquipmentSegment.SelectedValue = _LineID;
                    cboEquipmentSegment.IsEnabled = false;
                }

                //공정
                if (cboProcess != null && cboProcess.Items != null && cboProcess.Items.Count > 0)
                {
                    cboProcess.SelectedValue = _ProcID;

                    if (!cboEquipmentSegment.IsEnabled)
                        cboProcess.IsEnabled = false;
                }

                //설비
                if (cboEquipment != null && cboEquipment.Items != null && cboEquipment.Items.Count > 0)
                {
                    cboEquipment.SelectedValue = _EqptID;

                    if (!cboEquipmentSegment.IsEnabled && !cboProcess.IsEnabled)
                        cboEquipment.IsEnabled = false;
                }

                if (_ProcID.Equals(Process.PACKAGING))
                {
                    if (dgEqptCond.Columns.Contains("UNIT_EQPTNAME"))
                        dgEqptCond.Columns["UNIT_EQPTNAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgEqptCond.Columns.Contains("UNIT_EQPTNAME"))
                        dgEqptCond.Columns["UNIT_EQPTNAME"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "7");
                return;
            }

            InitializeGrid(dg: dgEqptCond);
            GetEqptPrdtGetItem();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void dgEqptCond_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqptCond_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("INPUT_VALUE"))
                    {
                        if (dataGrid.Columns.Contains("DATA_TYPE"))
                        {
                            string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DATA_TYPE"));
                            if (sDataType.Equals("A"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            else if (sDataType.Equals("B"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgEqptCond_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            InitializeGrid(dg: dgEqptCond);
            this.Focus();
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            InitializeGrid(dg: dgEqptCond);
            this.Focus();
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                InitializeGrid(dg: dgEqptCond);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                InitializeGrid(dg: dgEqptCond);

                string sEqsg = cboEquipmentSegment != null && cboEquipmentSegment.SelectedValue != null ? Util.NVC(cboEquipmentSegment.SelectedValue) : "";
                string sProc = cboProcess != null && cboProcess.SelectedValue != null ? Util.NVC(cboProcess.SelectedValue) : "";
                const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
                string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, sEqsg, sProc };
                string selectedValueText = cboShift.SelectedValuePath;
                string displayMemberText = cboShift.DisplayMemberPath;
                CommonCombo.CommonBaseCombo(bizRuleName, cboShift, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                InitializeGrid(dg: dgEqptCond);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboShift_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                InitializeGrid(dg: dgEqptCond);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            //저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetEqptCond();
                }
            });
        }

        private bool CanSave()
        {
            bool bRet = false;

            if (dgEqptCond.ItemsSource == null || dgEqptCond.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1651");      //선택된 항목이 없습니다.
                return bRet;
            }

            //if (_LotID.Trim().Length < 1)
            //{
            //    Util.MessageValidation("SFU1195");      //Lot 정보가 없습니다.
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private void SetEqptCond()
        {
            try
            {
                ShowLoadingIndicator();

                dgEqptCond.EndEdit();

                // 변경된 값이 존재하는 LOT 조회
                DataTable dtOrgData = DataTableConverter.Convert(dgEqptCond.ItemsSource);
                DataRow[] drChgData = dtOrgData?.Select("INPUT_VALUE_ORG <> INPUT_VALUE");
                if (drChgData?.Length < 1) return;

                DataTable dtChgLotData = drChgData?.AsEnumerable()?.GroupBy(r => r.Field<string>("LOTID"))?.Select(g => g.First()).CopyToDataTable();

                if (dtChgLotData?.Rows?.Count < 1) return;

                for (int z = 0; z < dtChgLotData.Rows.Count; z++)
                {
                    DataTable dtSource = dtOrgData?.Select("LOTID = '" + Util.NVC(dtChgLotData.Rows[z]["LOTID"]) + "'", "LOTID ASC, UNIT_EQPTID ASC").CopyToDataTable();
             
                    #region LOT 별 저장 처리
                    if (_ProcID.Equals(Process.PACKAGING))
                    {
                        #region Packaging
                        DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                        DataTable inTable = indataSet.Tables["IN_EQP"];

                        DataRow newRow = null;

                        //DataRow newRow = inTable.NewRow();
                        //newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        //newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        //newRow["EQPTID"] = _EqptID;
                        //newRow["UNIT_EQPTID"] = _EqptID;
                        //newRow["USERID"] = LoginInfo.USERID;
                        //newRow["LOTID"] = _LotID;

                        //inTable.Rows.Add(newRow);

                        DataTable in_Data = indataSet.Tables["IN_DATA"];

                        // Biz Core Multi 처리 없으므로 임시로 Unit 단위로 비즈 호출 처리 함.
                        // 추후 Multi Biz 생성 시 처리 방법 변경 필요.
                        string sUnitID = "";
                        for (int i = 0; i < dtSource.Rows.Count; i++)
                        {
                            string sTmp = Util.NVC(dtSource.Rows[i]["UNIT_EQPTID"]);

                            if (i == 0)
                            {
                                sUnitID = sTmp;

                                newRow = null;

                                newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                newRow["EQPTID"] = _EqptID;
                                newRow["UNIT_EQPTID"] = sUnitID;
                                newRow["USERID"] = LoginInfo.USERID;
                                newRow["LOTID"] = Util.NVC(dtChgLotData.Rows[z]["LOTID"]);
                                newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                                inTable.Rows.Add(newRow);

                                newRow = null;

                                newRow = in_Data.NewRow();
                                newRow["CLCTITEM"] = Util.NVC(dtSource.Rows[i]["CLCTITEM"]);
                                newRow["CLCTITEM_VALUE01"] = Util.NVC(dtSource.Rows[i]["INPUT_VALUE"]);

                                in_Data.Rows.Add(newRow);
                            }
                            else
                            {
                                if (sUnitID.Equals(sTmp))
                                {
                                    newRow = null;

                                    newRow = in_Data.NewRow();
                                    newRow["CLCTITEM"] = Util.NVC(dtSource.Rows[i]["CLCTITEM"]);
                                    newRow["CLCTITEM_VALUE01"] = Util.NVC(dtSource.Rows[i]["INPUT_VALUE"]);

                                    in_Data.Rows.Add(newRow);
                                }
                                else
                                {
                                    // data 존재 시 biz call
                                    if (inTable.Rows.Count > 0 && in_Data.Rows.Count > 0)
                                    {
                                        new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                                        inTable.Rows.Clear();
                                        in_Data.Rows.Clear();

                                        //Util.AlertInfo("SFU1275");      //정상 처리 되었습니다.

                                        //GetEqptCondInfo();
                                    }

                                    sUnitID = sTmp;

                                    newRow = null;

                                    newRow = inTable.NewRow();
                                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                    newRow["EQPTID"] = _EqptID;
                                    newRow["UNIT_EQPTID"] = sUnitID;
                                    newRow["USERID"] = LoginInfo.USERID;
                                    newRow["LOTID"] = Util.NVC(dtChgLotData.Rows[z]["LOTID"]);
                                    newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                                    inTable.Rows.Add(newRow);

                                    newRow = null;

                                    newRow = in_Data.NewRow();
                                    newRow["CLCTITEM"] = Util.NVC(dtSource.Rows[i]["CLCTITEM"]);
                                    newRow["CLCTITEM_VALUE01"] = Util.NVC(dtSource.Rows[i]["INPUT_VALUE"]);

                                    in_Data.Rows.Add(newRow);
                                }
                            }
                        }

                        // 마지막 Unit 처리.
                        if (inTable.Rows.Count > 0 && in_Data.Rows.Count > 0)
                        {
                            new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                            inTable.Rows.Clear();
                            in_Data.Rows.Clear();
                            
                            //Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                            //GetEqptCondInfo();
                        }
                        #endregion
                    }
                    else if (_ProcID.Equals(Process.NOTCHING))
                    {
                        #region Notching
                        //if (_LotID2.Equals(""))
                        //{
                        DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                            DataTable inTable = indataSet.Tables["IN_EQP"];

                            DataRow newRow = inTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            newRow["EQPTID"] = _EqptID;
                            //newRow["UNIT_EQPTID"] = _EqptID;
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["LOTID"] = Util.NVC(dtChgLotData.Rows[z]["LOTID"]);
                            newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                            inTable.Rows.Add(newRow);

                            DataTable in_Data = indataSet.Tables["IN_DATA"];

                            for (int i = 0; i < dtSource.Rows.Count; i++)
                            {
                                newRow = null;

                                newRow = in_Data.NewRow();
                                newRow["CLCTITEM"] = Util.NVC(dtSource.Rows[i]["CLCTITEM"]);
                                newRow["CLCTITEM_VALUE01"] = Util.NVC(dtSource.Rows[i]["INPUT_VALUE"]);

                                in_Data.Rows.Add(newRow);
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                            inTable.Rows.Clear();
                            in_Data.Rows.Clear();

                            //Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
                                                              //}
                                                              //else
                                                              //{
                                                              //    // 2 건 저장
                                                              //    DataSet indataSet = new DataSet();

                        //    DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                        //    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        //    inDataTable.Columns.Add("IFMODE", typeof(string));
                        //    inDataTable.Columns.Add("EQPTID", typeof(string));
                        //    inDataTable.Columns.Add("USERID", typeof(string));
                        //    //inDataTable.Columns.Add("SUBLOTID", typeof(string));
                        //    inDataTable.Columns.Add("INPUT_SEQ_NO", typeof(int));
                        //    //inDataTable.Columns.Add("EVENT_NAME", typeof(string));

                        //    DataTable in_DATA = indataSet.Tables.Add("IN_DATA");
                        //    in_DATA.Columns.Add("LOTID", typeof(string));
                        //    in_DATA.Columns.Add("CLCTITEM", typeof(string));
                        //    in_DATA.Columns.Add("CLCTITEM_VALUE01", typeof(string));


                        //    DataTable inTable = indataSet.Tables["IN_EQP"];

                        //    DataRow newRow = inTable.NewRow();
                        //    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        //    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        //    newRow["EQPTID"] = _EqptID;
                        //    newRow["USERID"] = LoginInfo.USERID;
                        //    newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                        //    inTable.Rows.Add(newRow);

                        //    DataTable in_Data = indataSet.Tables["IN_DATA"];

                        //    for (int i = 0; i < dtSource.Rows.Count; i++)
                        //    {
                        //        newRow = null;

                        //        newRow = in_Data.NewRow();
                        //        newRow["LOTID"] = Util.NVC(dtChgLotData.Rows[z]["LOTID"]);
                        //        newRow["CLCTITEM"] = Util.NVC(dtSource.Rows[i]["CLCTITEM"]);
                        //        newRow["CLCTITEM_VALUE01"] = Util.NVC(dtSource.Rows[i]["INPUT_VALUE"]);

                        //        in_Data.Rows.Add(newRow);

                        //        newRow = null;

                        //        newRow = in_Data.NewRow();
                        //        newRow["LOTID"] = _LotID2;
                        //        newRow["CLCTITEM"] = Util.NVC(dtSource.Rows[i]["CLCTITEM"]);
                        //        newRow["CLCTITEM_VALUE01"] = Util.NVC(dtSource.Rows[i]["INPUT_VALUE"]);

                        //        in_Data.Rows.Add(newRow);
                        //    }

                        //    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT_S_NT", "IN_EQP,IN_DATA", null, indataSet);

                        //    inTable.Rows.Clear();
                        //    in_Data.Rows.Clear();

                        //    Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        //}
                        #endregion
                    }
                    else
                    {
                        #region 그 외 공정
                        DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                        DataTable inTable = indataSet.Tables["IN_EQP"];

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _EqptID;
                        //newRow["UNIT_EQPTID"] = _EqptID;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["LOTID"] = Util.NVC(dtChgLotData.Rows[z]["LOTID"]);
                        newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                        inTable.Rows.Add(newRow);

                        DataTable in_Data = indataSet.Tables["IN_DATA"];

                        for (int i = 0; i < dtSource.Rows.Count; i++)
                        {
                            newRow = null;

                            newRow = in_Data.NewRow();
                            newRow["CLCTITEM"] = Util.NVC(dtSource.Rows[i]["CLCTITEM"]);
                            newRow["CLCTITEM_VALUE01"] = Util.NVC(dtSource.Rows[i]["INPUT_VALUE"]);

                            in_Data.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                        inTable.Rows.Clear();
                        in_Data.Rows.Clear();

                        //Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        //GetEqptCondInfo();

                        #endregion
                    }
                    #endregion
                }

                Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                GetEqptPrdtGetItem();

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

        private void dgEqptCond_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null)
                    return;

                int iStdx = 0;
                int iEndx = 0;
                string sTmpLotID = string.Empty;
                string sTmpDttm = string.Empty;
                for (int i = 0; i < dg.Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        iStdx = i;
                        sTmpLotID = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID"));
                        sTmpDttm = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "INPUT_DATE"));

                        continue;
                    }

                    if (sTmpLotID.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID"))))
                    {
                        iEndx = i;

                        if (!sTmpDttm.Equals("") && !sTmpDttm.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "INPUT_DATE"))))
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, "INPUT_DATE", sTmpDttm);
                    }
                    else
                    {
                        if (iStdx < iEndx && !Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "INPUT_DATE")).Equals(""))
                            e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["INPUT_DATE"].Index), dg.GetCell(iEndx, dg.Columns["INPUT_DATE"].Index)));

                        iStdx = i;
                        sTmpLotID = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID"));
                        sTmpDttm = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "INPUT_DATE"));
                    }
                }

                if (iStdx < iEndx && !sTmpDttm.Equals("") && !sTmpDttm.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[iEndx].DataItem, "INPUT_DATE"))))
                    DataTableConverter.SetValue(dg.Rows[iEndx].DataItem, "INPUT_DATE", sTmpDttm);

                if (iStdx < iEndx && !Util.NVC(DataTableConverter.GetValue(dg.Rows[iStdx].DataItem, "INPUT_DATE")).Equals(""))
                    e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["INPUT_DATE"].Index), dg.GetCell(iEndx, dg.Columns["INPUT_DATE"].Index)));
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}