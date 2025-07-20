/*************************************************************************************
 Created Date : 2019.04.04
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.04  정종원 : 신규로 전극 수율 UI 분리
  2021.07.15  김지은 : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
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
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_257 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private string _prodLotId = string.Empty;
        private string _prodWipSeq = string.Empty;
        private string _MTRLID = string.Empty;
        private string _pcsgID = string.Empty;
        private bool _isLoad = true;

        public COM001_257()
        {
            InitializeComponent();
            InitializeCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeCombo()
        {
            CommonCombo cbo = new CommonCombo();

            // 동 정보 조회
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            cbo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);
            // 라인 정보 조회
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            cbo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);
            // 공정 정보 조회
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            cbo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);
            if (cboProcess.Items.Count < 1)
                SetProcess();

            // 설비 정보 조회
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            cbo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;

            cbo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            string[] sFilter = { "APPR_BIZ_CODE" };
            cbo.SetCombo(cboReqTypeHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            string[] sFilter1 = { "REQ_RSLT_CODE" };
            cbo.SetCombo(cboReqRsltHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            cboReqTypeHist.SelectedIndex = 4;
            cboReqTypeHist.IsEditable = false;
            cboReqTypeHist.IsEnabled = false;

            //생산구분
            string[] sFilter2 = new string[] { "PRODUCT_DIVISION" };
            cbo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

            // 공정 초기 Set
            if (string.Equals(cboProcess.SelectedValue, Process.COATING))
            {
                cboEquipment.SelectedIndex = -1;
                cboEquipment.IsEnabled = false;
                DateGubun.Visibility = Visibility.Visible;

                dgLotList.Columns["LOTID"].Visibility = Visibility.Visible;
                dgLotList.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LOTYNAME"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["MKT_TYPE_CODE"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["MTRLID"].Visibility = Visibility.Visible;
                dgLotList.Columns["CONVER_RATE"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["INPUT_QTY_TOP_BACK"].Visibility = Visibility.Visible;
                dgLotList.Columns["INPUT_QTY_TOP"].Visibility = Visibility.Visible;

            }
            else
            {
                cboEquipment.IsEnabled = true;
                DateGubun.Visibility = Visibility.Hidden;

                dgLotList.Columns["LOTID"].Visibility = Visibility.Visible;
                dgLotList.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                //dgLotList.Columns["LOTYNAME"].Visibility = Visibility.Visible;
                dgLotList.Columns["MKT_TYPE_CODE"].Visibility = Visibility.Visible;
                dgLotList.Columns["MTRLID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["CONVER_RATE"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["INPUT_QTY_TOP_BACK"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["INPUT_QTY_TOP"].Visibility = Visibility.Collapsed;
            }

            // 길이초과, 길이부족 추가 [2019-04-23]
            if (string.Equals(cboProcess.SelectedValue, Process.INS_COATING) || string.Equals(cboProcess.SelectedValue, Process.HALF_SLITTING) ||
                string.Equals(cboProcess.SelectedValue, Process.ROLL_PRESSING) || string.Equals(cboProcess.SelectedValue, Process.TAPING) ||
                string.Equals(cboProcess.SelectedValue, Process.SLITTING))
            {
                dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Visible;
            }
            else
            {
                dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            GetCaldate();

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotList();
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetProcess();
            }
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgLotList);
                ClearValue();

                // 코터는 제품 단위 수율을 포함하기위하여 추가 [2019-04-04]
                if (string.Equals(cboProcess.SelectedValue, Process.COATING))
                {
                    cboEquipment.SelectedIndex = -1;
                    cboEquipment.IsEnabled = false;
                    DateGubun.Visibility = Visibility.Visible;

                    dgLotList.Columns["LOTID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["LOTYNAME"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["MKT_TYPE_CODE"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["MTRLID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CONVER_RATE"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INPUT_QTY_TOP_BACK"].Visibility = Visibility.Visible;
                    dgLotList.Columns["INPUT_QTY_TOP"].Visibility = Visibility.Visible;
                }
                else
                {
                    cboEquipment.IsEnabled = true;
                    DateGubun.Visibility = Visibility.Hidden;

                    dgLotList.Columns["LOTID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                    dgLotList.Columns["LOTYNAME"].Visibility = Visibility.Visible;
                    dgLotList.Columns["MKT_TYPE_CODE"].Visibility = Visibility.Visible;
                    dgLotList.Columns["MTRLID"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["CONVER_RATE"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INPUT_QTY_TOP_BACK"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INPUT_QTY_TOP"].Visibility = Visibility.Collapsed;


                    // 코터 아닐 경우 다시 설정
                    if (rboMonth.IsChecked == true)
                    {
                        rboDay.IsChecked = true;
                        dtpDateFrom.DatepickerType = LGCDataPickerType.Date;
                        dtpDateFrom.SelectedDateTime = DateTime.Now;
                        dtpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    }

                    dtpGubun.Visibility = Visibility.Visible;
                    dtpDateTo.Visibility = Visibility.Visible;
                }

                // 길이초과, 길이부족 추가 [2019-04-23]
                if (string.Equals(cboProcess.SelectedValue, Process.INS_COATING) || string.Equals(cboProcess.SelectedValue, Process.HALF_SLITTING) ||
                    string.Equals(cboProcess.SelectedValue, Process.ROLL_PRESSING) || string.Equals(cboProcess.SelectedValue, Process.TAPING) ||
                    string.Equals(cboProcess.SelectedValue, Process.SLITTING))
                {
                    dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                cboEquipment.IsEnabled = true;
                DateGubun.Visibility = Visibility.Hidden;

                dgLotList.Columns["LOTID"].Visibility = Visibility.Visible;
                dgLotList.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                dgLotList.Columns["LOTYNAME"].Visibility = Visibility.Visible;
                dgLotList.Columns["MKT_TYPE_CODE"].Visibility = Visibility.Visible;
                dgLotList.Columns["MTRLID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["CONVER_RATE"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Collapsed;
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateTo.Visibility == Visibility.Visible)
            {
                if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
                {
                    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                    {
                        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                        return;
                    }
                }
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        private void txtModlId_GotFocus(object sender, RoutedEventArgs e)
        {
            // 모델 AutoComplete
            if (_isLoad)
            {
                GetModel();
                _isLoad = false;
            }
        }

        private void txtPrjtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        /*
        private void txtDIFF_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        */

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (rb != null && DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);
                GetSubLot();
                GetInputHistory();
            }
        }


        #region [완성LOT 탭] - dgSubLot_LoadedCellPresenter, dgSubLot_MouseDoubleClick(셀정보팝업), print_Button_Click(재발행)
        private void dgSubLot_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (cboProcess.SelectedValue.Equals(Process.PACKAGING) && e.Cell.Column.Name.Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));

        }
        #endregion




        #endregion

        #region Mehod

        #region [BizCall]


        private void GetCaldate()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; // 2024.10.02 김영국 - Oracle Query상 AREAID BINDING변수가 빠져있어 추가함.
                dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetLotList()
        {
            try
            {
                if (dtpDateTo.Visibility == Visibility.Visible)
                {
                    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                    {
                        //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                        Util.MessageValidation("SFU2042", "31");
                        return;
                    }
                }

                ShowLoadingIndicator();
                DoEvents();

                string bizRuleName = "";
                if (string.Equals(cboProcess.SelectedValue, Process.COATING) && rboMonth.IsChecked == true)
                    bizRuleName = "DA_PRD_SEL_LOT_LIST_INPUT_MATERIAL_ELEC_CT";
                else if (string.Equals(cboProcess.SelectedValue, Process.COATING) && rboDay.IsChecked == true)
                    bizRuleName = "DA_PRD_SEL_LOT_LIST_INPUT_MATERIAL_ELEC_CT_YYMMDD";
                else
                    bizRuleName = "DA_PRD_SEL_LOT_LIST_INPUT_MATERIAL_ELEC";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("DIFF", typeof(Int16));
                //dtRqst.Columns.Add("NORMAL", typeof(string));
                //dtRqst.Columns.Add("PILOT", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;

                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                if (string.Equals(cboProcess.SelectedValue, Process.COATING) && rboMonth.IsChecked == true )
                {
                    dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM");
                }
                else if (string.Equals(cboProcess.SelectedValue, Process.COATING) && rboDay.IsChecked == true)
                {
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                }
                else
                {
                    string equipment = Util.GetCondition(cboEquipment);
                    dr["EQPTID"] = string.IsNullOrWhiteSpace(equipment) ? null : equipment;

                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                }

                if (!string.IsNullOrEmpty(txtLotId.Text))
                    dr["LOTID"] = Util.GetCondition(txtLotId);

                if (!string.IsNullOrWhiteSpace(txtModlId.Text))
                    dr["MODLID"] = txtModlId.Text;

                if (!string.IsNullOrWhiteSpace(txtProdId.Text))
                    dr["PRODID"] = txtProdId.Text;

                if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
                //if (cboProductDiv.SelectedValue.ToString() == "P")
                //    dr["NORMAL"] = cboProductDiv.SelectedValue.ToString();
                //else if (cboProductDiv.SelectedValue.ToString() == "X")
                //    dr["PILOT"] = cboProductDiv.SelectedValue.ToString();
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);

                //if (txtDIFF.Value > 0)
                //    dr["DIFF"] = txtDIFF.Value;

                dtRqst.Rows.Add(dr);


                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgLotList, searchResult, FrameOperation, true);

                        string[] sColumnName = { "WOID" };
                        _util.SetDataGridMergeExtensionCol(dgLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                        //if (string.Equals(cboProcess.SelectedValue, Process.COATING))
                        //{
                        //    string[] sColumnName = { "LOTID", "PRODID", "PRODNAME", "MODLID", "PRJT_NAME", "EQPTNAME", "CALDATE", "LOTYNAME", "MKT_TYPE_CODE", "UNIT_CODE", "INPUT_QTY", "WIPQTY_ED", "CNFM_DFCT_QTY", "CNFM_LOSS_QTY", "CNFM_PRDT_REQ_QTY", "NON_LOSS_QTY", "LENGTH_EXCEED", "WOID" };
                        //    _util.SetDataGridMergeExtensionCol(dgLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                        //}
                        //else
                        //{
                        //    string[] sColumnName = { "LOTID", "PRODID", "PRODNAME", "MODLID", "PRJT_NAME", "EQPTNAME", "CALDATE", "LOTYNAME", "MKT_TYPE_CODE", "UNIT_CODE", "WOID" };
                        //    _util.SetDataGridMergeExtensionCol(dgLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                        //}
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
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSubLot()
        {
            try
            {
                /*
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = _prodLotId;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EDIT_SUBLOT_LIST", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgSubLot, dtRslt, FrameOperation);
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputHistory()
        {
            /*
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WIPHISTORYATTR_INPUT_MTRL";

                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("MTRLID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = _prodLotId;
                newRow["PROD_WIPSEQ"] = _prodWipSeq;
                newRow["MTRLID"] = _MTRLID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInputHistory, searchResult, FrameOperation);

                        if (dgInputHistory.CurrentCell != null)
                            dgInputHistory.CurrentCell = dgInputHistory.GetCell(dgInputHistory.CurrentCell.Row.Index, dgInputHistory.Columns.Count - 1);
                        else if (dgInputHistory.Rows.Count > 0)
                            dgInputHistory.CurrentCell = dgInputHistory.GetCell(dgInputHistory.Rows.Count, dgInputHistory.Columns.Count - 1);
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
                Util.MessageException(ex);
            }
            */
        }

        private void GetModel()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL", "INDATA", "OUTDATA", inTable);

                foreach (DataRow r in dtRslt.Rows)
                {
                    string displayString = r["MODLID"].ToString(); //표시 텍스트
                    var keywordString = displayString;
                    txtModlId.AddItem(new AutoCompleteEntry(displayString, keywordString)); //표시 텍스트와 검색어 텍스트(배열)를 AutoCompleteTextBox의 Item에 추가한다.
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
        }

        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string area = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(area))
                    return;

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = area;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", inTable);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";
                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string area = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(area))
                    return;

                string processCode = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = area;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void ClearValue()
        {
            _prodLotId = string.Empty;
            _prodWipSeq = string.Empty;
            _MTRLID = string.Empty;

            Util.gridClear(dgInputHistory1);
            Util.gridClear(dgInputHistory2);
        }


        private void SetValue(object oContext)
        {
            _prodLotId = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _prodWipSeq = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _MTRLID = Util.NVC(DataTableConverter.GetValue(oContext, "MTRLID"));
        }


        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #endregion

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();
            DoEvents();
            GetListHist();
            HiddenLoadingIndicator();
        }
        public void GetListHist()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (Util.GetCondition(txtLotIDHist).Equals("")) //lot id 가 없는 경우
                {

                    dr["AREAID"] = Util.GetCondition(cboAreaHist, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["USERNAME"] = Util.GetCondition(txtReqUserHist);
                    dr["APPR_BIZ_CODE"] = Util.GetCondition(cboReqTypeHist, bAllNull: true);
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRsltHist, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;

                    if (!Util.GetCondition(txtCSTIDHist).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTIDHist);

                    dtRqst.Rows.Add(dr);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotIDHist);

                    dtRqst.Rows.Add(dr);
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_HIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgListHist, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgListHist_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgListHist.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("LOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

            }));
        }

        private void dgListHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgListHist.CurrentRow != null && dgListHist.CurrentColumn.Name.Equals("LOTID") && dgListHist.GetRowCount() > 0)
            {

                COM001_035_READ wndPopup = new COM001_035_READ();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgListHist.CurrentRow.DataItem, "REQ_NO"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgListHist.CurrentRow.DataItem, "REQ_RSLT_CODE"));


                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
        }

        private void rdoButton_Click(object sender, RoutedEventArgs e)
        {
            if (rboDay.IsChecked == true)
            {
                dtpDateFrom.DatepickerType = LGCDataPickerType.Date;
                dtpDateFrom.SelectedDateTime = DateTime.Now;
                dtpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
                dtpGubun.Visibility = Visibility.Visible;
                dtpDateTo.Visibility = Visibility.Visible;
            }
            else if (rboMonth.IsChecked == true)
            {
                dtpDateFrom.DatepickerType = LGCDataPickerType.Month;
                dtpDateFrom.SelectedDateTime = DateTime.Now;
                dtpDateFrom.Text = DateTime.Now.ToString("yyyy-MM");
                dtpGubun.Visibility = Visibility.Hidden;
                dtpDateTo.Visibility = Visibility.Hidden;
            }
        }
    }
}