/*************************************************************************************
 Created Date : 2021.11.11
      Creator : 
   Decription : 샘플검사 처리
--------------------------------------------------------------------------------------
 [Change History]
 2021.11.11    강호운   신규개발
 2021.12.15    강호운   컬럼 추가 (jig, rackid, stk 재공 존재여부)
 2022.03.03    김길용   수동그룹핑버튼 추가 (btnManualInput)
 2024.09.02    최평부   ESST VD590 라인 증설 SI 조회 리스트 에 PALLETID 컬럼 추가
 2024.11.15    최평부   ESST CMA Batch 취소 기능 추가
**************************************************************************************/

using System.Windows.Controls;
using LGC.GMES.MES.Common;
using System;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Input;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.COM001;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_029 : UserControl, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        int TotalRow;
        bool HoldChk = false;

        int iReleaseRow;
        private bool _manualCommit = false;

        private object lockObject = new object();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_029()
        {
            InitializeComponent();
            intHoldCombo();
            GetAreaDefectCode();
            cboEquipmentSegment.SelectedIndex = 0;
            this.grOKGrid.Visibility = Visibility.Collapsed;
            this.grNGGrid.Visibility = Visibility.Collapsed;
            grSampleJudg.Visibility = Visibility.Collapsed;
            grReJudg.Visibility = Visibility.Collapsed;
            //2024-11-18 by 최평부
            //diffusion_site 공통코드 조회(화면 : SITE별 분기 처리)
            DataTable dtDiffusionSite = new DataTable();
            dtDiffusionSite = GetCommonCode("DIFFUSION_SITE", "AUTO_LOGIS");

            string shop_id = string.Empty;

            if (dtDiffusionSite.Rows.Count > 0)
            {
                shop_id = dtDiffusionSite.Rows[0]["ATTRIBUTE1"].ToString();

                if (shop_id.Contains(LoginInfo.CFG_SHOP_ID))
                {
                    this.btnManualInput.Visibility = Visibility.Collapsed;
                    this.btnSample.Visibility = Visibility.Collapsed;
                    this.btnGroupCancel.Visibility = Visibility.Visible;

                    this.grdLeft.Visibility = Visibility.Collapsed;
                    this.grdRight.Visibility = Visibility.Collapsed;
                }
            }
        }

        private DataTable GetCommonCode(string codeType, string code)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CBO_CODE"] = code;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return new DataTable();
        }

        public void intHoldCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboArea = new C1ComboBox();
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //라인            
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProductModel, cboProduct };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild, sCase: "EQUIPMENTSEGMENT");

            //모델          
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL");

            //제품코드  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            //HOLD 사유
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            //HOLD 사유
            string[] sFilter1 = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason1, CommonCombo.ComboStatus.ALL, sCase: "ACTIVITIREASON", sFilter: sFilter1);

            String[] sFilter3 = { "JUDGE_OK" };
            _combo.SetCombo(cboResult, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE");
            _combo.SetCombo(cboResult1, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE");

            setCombo();
            setQACombo();
            setCombo_targt_flag();

            //UNHOLD(해제)사유 
            string[] sFilter4 = { "UNHOLD_LOT" };
            _combo.SetCombo(cboUnHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter4);

            SetDataGridComboItem(CommonCombo.ComboStatus.NONE, dgCellHistory.Columns["SMPL_TRGT_FLAG"], "CBO_CODE", "CBO_NAME");

        }

        public static void SetDataGridComboItem(CommonCombo.ComboStatus status, C1.WPF.DataGrid.DataGridColumn dgcol, string selectedValueText, string displayMemberText)
        {
            try
            {

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CBO_NAME", typeof(string));
                inDataTable.Columns.Add("CBO_CODE", typeof(string));

                string[] stValue = new string[] { "N", "Y" };
                string[] stKey = new string[] { "미대상", "TARGET" };

                for (int i = 0; i < stKey.Count(); i++)
                {
                    DataRow dr0 = inDataTable.NewRow();
                    dr0["CBO_NAME"] = stValue[i] + " : " + ObjectDic.Instance.GetObjectName(stKey[i]);
                    dr0["CBO_CODE"] = stValue[i];
                    inDataTable.Rows.Add(dr0);
                }

                DataTable dtResult = inDataTable;

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
                C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = AddStatus(dtBinding, status, selectedValueText, displayMemberText).Copy().AsDataView();
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private static DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void setCombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("KEY", typeof(string));
                dt.Columns.Add("VALUE", typeof(string));

                DataRow dr0 = dt.NewRow();
                dr0["KEY"] = "-ALL-";
                dr0["VALUE"] = "";
                dt.Rows.Add(dr0);

                DataRow dr1 = dt.NewRow();
                dr1["KEY"] = "N : " + ObjectDic.Instance.GetObjectName("미대상");
                dr1["VALUE"] = "N";
                dt.Rows.Add(dr1);

                DataRow dr = dt.NewRow();
                dr["KEY"] = "Y : " + ObjectDic.Instance.GetObjectName("TARGET");
                dr["VALUE"] = "Y";
                dt.Rows.Add(dr);
                dt.AcceptChanges();

                cbo_SMPL_TRGT_FLAG.ItemsSource = DataTableConverter.Convert(dt);
                cbo_SMPL_TRGT_FLAG.SelectedIndex = 0; //default Y

                cbo_QA_INSP_TRGT_FLAG.ItemsSource = DataTableConverter.Convert(dt);
                cbo_QA_INSP_TRGT_FLAG.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setCombo_targt_flag()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("KEY", typeof(string));
                dt.Columns.Add("VALUE", typeof(string));

                string[] stValue = new string[] { "", "OK", "NG", "RD" };
                string[] stKey = new string[] { "-ALL-", "OK", "NG", "준비" };

                for (int i = 0; i < stKey.Count(); i++)
                {
                    DataRow dr0 = dt.NewRow();
                    dr0["KEY"] = (i == 0) ? stKey[i] : stValue[i] + " : " + ObjectDic.Instance.GetObjectName(stKey[i]);
                    dr0["VALUE"] = stValue[i];
                    dt.Rows.Add(dr0);
                }

                dt.AcceptChanges();

                cbo_SMPL_INSP_RSLT.ItemsSource = DataTableConverter.Convert(dt);
                cbo_SMPL_INSP_RSLT.SelectedIndex = 0; //default Y

                cbo_SMPL_GR_INSP_RSLT.ItemsSource = DataTableConverter.Convert(dt);
                cbo_SMPL_GR_INSP_RSLT.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setQACombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("KEY", typeof(string));
                dt.Columns.Add("VALUE", typeof(string));

                DataRow dr1 = dt.NewRow();
                dr1["KEY"] = "N : " + ObjectDic.Instance.GetObjectName("미대상");
                dr1["VALUE"] = "N";
                dt.Rows.Add(dr1);

                DataRow dr = dt.NewRow();
                dr["KEY"] = "Y : " + ObjectDic.Instance.GetObjectName("TARGET");
                dr["VALUE"] = "Y";
                dt.Rows.Add(dr);
                dt.AcceptChanges();

                cboQATarget.ItemsSource = DataTableConverter.Convert(dt);
                cboQATarget.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
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

        private void txtPerson_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnPerson_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellHistory.ItemsSource == null || dgCellHistory.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU3537");
                    return;
                }

                TextRange textRange = new TextRange(rtbHoldCompare.Document.ContentStart, rtbHoldCompare.Document.ContentEnd);

                if (cboResult.SelectedValue.Equals("NG"))
                {
                    //해체 예정 담당자 필수 : 
                    if (txtPerson.Tag == null || txtPerson.Tag.Equals("\r\n"))
                    {
                        Util.MessageInfo("SFU4350"); //해체 예정 담당자를 선택하세요.
                        return;
                    }

                    //홀드 사유 필수 : 
                    if (cboHoldReason.SelectedIndex < 1)
                    {
                        Util.MessageInfo("SFU1342"); //HOLD 사유를 선택 하세요.
                        return;
                    }

                    //HOLD 비고를 입력하세요    

                    if (textRange.Text.Equals("\r\n") || textRange.Text.Equals(""))
                    {
                        Util.MessageInfo("SFU1341");
                        return;
                    }
                }

                if (!rbSample.IsChecked.Value)
                {
                    Util.MessageInfo("SFU3370");  // 샘플lot 을 선택해주세요.
                    return;
                }

                HoldChk = false;

                setJudg(textRange.Text);

                txtSearchLotId.Text = "";
                rtbHoldCompare.Document.Blocks.Clear();
                TotalRow = 0;

                dgCellHistory.ItemsSource = null;
                getList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    TotalRow = 0;
                    Util.gridClear(dgCellHistory);
                }
            });
        }

        private void btnGroupCanCel_Click(object sender, RoutedEventArgs e)
        {
            // 팝업 호출
            PACK003_029_POPUP popup = new PACK003_029_POPUP { FrameOperation = FrameOperation };

            popup.Closed += popupCmdCancel_Closed;

            Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
        }

        private void popupCmdCancel_Closed(object sender, EventArgs e)
        {
            PACK003_029_POPUP popup = sender as PACK003_029_POPUP;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //btnSearch_Click(null, null);
            }
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton.DataContext == null)
                return;

            if ((bool)radioButton.IsChecked)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = (C1.WPF.DataGrid.DataGridCellPresenter)radioButton.Parent;
                if (cellPresenter != null)
                {
                    C1.WPF.DataGrid.C1DataGrid dataGrid = cellPresenter.DataGrid;
                    int rowIdx = cellPresenter.Row.Index;

                    if (string.Equals(radioButton.GroupName, "radHoldGroup1"))
                    {
                        if (cboHoldReason.Items.Count > 1)
                            cboHoldReason.SelectedIndex = 0;

                        GetAreaDefectDetailCode(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[rowIdx].DataItem, "DFCT_CODE")));
                    }
                    else if (string.Equals(radioButton.GroupName, "radHoldGroup2"))
                    {
                        cboHoldReason.SelectedValue = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[rowIdx].DataItem, "RESNCODE"));
                    }
                }
            }
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtPerson.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtPerson.Text = wndPerson.USERNAME;
                txtPerson.Tag = wndPerson.USERID;
            }
        }

        private void getList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SMPL_TRGT_FLAG", typeof(string));
                RQSTDT.Columns.Add("SMPL_INSP_RSLT", typeof(string));
                RQSTDT.Columns.Add("SMPL_GR_INSP_RSLT", typeof(string));
                RQSTDT.Columns.Add("QA_INSP_TRGT_FLAG", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("GLOTID", typeof(string));
                RQSTDT.Columns.Add("RESNCODE", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);
                dr["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct);
                dr["SMPL_TRGT_FLAG"] = Util.GetCondition(cbo_SMPL_TRGT_FLAG) == "" ? null : Util.GetCondition(cbo_SMPL_TRGT_FLAG);
                dr["SMPL_INSP_RSLT"] = Util.GetCondition(cbo_SMPL_INSP_RSLT) == "" ? null : Util.GetCondition(cbo_SMPL_INSP_RSLT);
                dr["SMPL_GR_INSP_RSLT"] = Util.GetCondition(cbo_SMPL_GR_INSP_RSLT) == "" ? null : Util.GetCondition(cbo_SMPL_GR_INSP_RSLT);
                dr["QA_INSP_TRGT_FLAG"] = Util.GetCondition(cbo_QA_INSP_TRGT_FLAG) == "" ? null : Util.GetCondition(cbo_QA_INSP_TRGT_FLAG);
                dr["RESNCODE"] = Util.GetCondition(cboHoldReason1) == "" ? null : Util.GetCondition(cboHoldReason1);

                if (string.IsNullOrWhiteSpace(txtSearchLotId.Text))
                {
                    txtSearchLotId.Text = null;
                }

                if (ckGroup.IsChecked.Value)
                {
                    dr["GLOTID"] = txtSearchLotId.Text == "" ? null : txtSearchLotId.Text;
                }
                else
                {
                    dr["LOTID"] = txtSearchLotId.Text == "" ? null : txtSearchLotId.Text;
                }

                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                dr["TODATE"] = Util.GetCondition(dtpDateTo);

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_SAMPLE_LOT_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgCellHistory, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtResult.Rows.Count));
                    }
                    else
                    {
                        Util.gridClear(dgCellHistory);
                        Util.SetTextBlockText_DataGridRowCount(tbCellListCount, "0");
                        Util.MessageInfo("SFU3537");
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private DataRow CellGridAdd(DataTable dt)
        {
            try
            {
                if (TotalRow == 0)
                {
                    Util.GridSetData(dgCellHistory, dt, FrameOperation);
                    TotalRow = dt.Rows.Count;
                    //++TotalRow;
                    return null;
                }

                DataRow dr = dt.NewRow();

                TotalRow = 0;
                Util.gridClear(dgCellHistory);

                Util.DataGridRowAdd(dgCellHistory, 1);

                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "CHK", dt.Rows[0]["CHK"].ToString());
                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "EQSGNAME", dt.Rows[0]["EQSGNAME"].ToString());
                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "EQSGID", dt.Rows[0]["EQSGID"].ToString());

                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "PROCNAME", dt.Rows[0]["PROCNAME"].ToString());
                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "WIPSNAME", dt.Rows[0]["WIPSNAME"].ToString());

                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "PRODID", dt.Rows[0]["PRODID"].ToString());
                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "WOID", dt.Rows[0]["WOID"].ToString());
                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "PRODNAME", dt.Rows[0]["PRODNAME"].ToString());
                DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow].DataItem, "WIPSTAT", dt.Rows[0]["WIPSTAT"].ToString());


                ++TotalRow;

                //  Util.SetTextBlockText_DataGridRowCount(tbCellInput_cnt, Convert.ToString(TotalRow));

                return dr;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void setJudg(string bigo)
        {
            try
            {
                if (dgCellHistory == null)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("IFMODE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("ACTION_USERID", typeof(string));
                INDATA.Columns.Add("CALDATE", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("HOLD_NOTE", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("HOLD_CODE", typeof(string));
                INDATA.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
                INDATA.Columns.Add("JUDG_FLAG", typeof(string));
                INDATA.Columns.Add("QA_INSP_TRGT_FLAG", typeof(string));
                INDATA.Columns.Add("SMPL_GR_ID", typeof(string));

                int chk_idx = 0;
                for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
                {

                    if (DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow dr = INDATA.NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["IFMODE"] = "OFF"; //설비에서 HOLD 처리하는지 여부 : UI-OFF
                        dr["USERID"] = LoginInfo.USERID;
                        dr["ACTION_USERID"] = txtPerson.Tag;
                        dr["LOTID"] = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "LOTID").ToString();

                        if (!(cboResult.SelectedValue.Equals("OK")))
                        {
                            dr["CALDATE"] = dtpCalDate.SelectedDateTime.ToString("yyyy-MM-dd");
                            dr["HOLD_NOTE"] = bigo.Replace("\r\n", "").Trim();
                            dr["RESNCODE"] = Util.GetCondition(cboHoldReason) == "" ? null : cboHoldReason.SelectedValue;
                            dr["HOLD_CODE"] = Util.GetCondition(cboHoldReason) == "" ? null : cboHoldReason.SelectedValue;
                            dr["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtpDate);
                        }
                        dr["JUDG_FLAG"] = cboResult.SelectedValue;
                        dr["QA_INSP_TRGT_FLAG"] = cboQATarget.SelectedValue;
                        dr["SMPL_GR_ID"] = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "SMPL_GR_ID").ToString();
                        INDATA.Rows.Add(dr);
                    }

                    chk_idx++;
                }

                if (chk_idx == 0)
                {
                    return;
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LOGIS_SAMPLE_LOT_JUDG", "INDATA", null, INDATA);
                Util.MessageInfo("SFU1889"); //HOLD 완료
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

        private DataTable getList(string LOTID)
        {
            try
            {

                DataTable INDATA = new DataTable();

                INDATA.Columns.Add("LANGID", typeof(string));

                if (ckGroup.IsChecked.Value)
                {
                    INDATA.Columns.Add("GLOTID", typeof(string));
                }
                else
                {
                    INDATA.Columns.Add("LOTID", typeof(string));
                }

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (ckGroup.IsChecked.Value)
                {
                    dr["GLOTID"] = LOTID;
                }
                else
                {
                    dr["LOTID"] = LOTID;
                }

                INDATA.Rows.Add(dr);
                DataTable OUTDATA = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_SAMPLE_LOT_LIST", "RQSTDT", "RSLTDT", INDATA, null);
                return OUTDATA;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }


        private void GetAreaDefectCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dataRow["ACTID"] = "HOLD_LOT";

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_HOLD_DFCT_CODE", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgHoldGroup1, result, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetAreaDefectDetailCode(string sDefectCode)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("DFCT_CODE", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dataRow["ACTID"] = "HOLD_LOT";
                dataRow["DFCT_CODE"] = sDefectCode;

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_HOLD_DFCT_DETL_CODE", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgHoldGroup2, result, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgCellHistory);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void cboResult_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            string s = e.NewValue.ToString();


            bool chk_vali = false;
            for (int i = 0; i < dgCellHistory.Rows.Count; i++)
            {
                var chkYn = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK");

                if (chkYn != null)
                {
                    if (Convert.ToBoolean(chkYn))
                    {
                        chk_vali = true;
                    }
                }
            }

            switch (s)
            {
                case "0":
                    this.grOKGrid.Visibility = Visibility.Collapsed;
                    this.grNGGrid.Visibility = Visibility.Collapsed;
                    break;
                case "1":
                    if (!chk_vali)
                    {
                        ms.AlertWarning("SFU1651"); //선택된 항목이 없습니다.
                        this.grOKGrid.Visibility = Visibility.Collapsed;
                        this.grNGGrid.Visibility = Visibility.Collapsed;
                        return;
                    }

                    this.grOKGrid.Visibility = Visibility.Visible;
                    this.grNGGrid.Visibility = Visibility.Collapsed;
                    break;
                case "2":
                    if (!chk_vali)
                    {
                        ms.AlertWarning("SFU1651"); //선택된 항목이 없습니다.
                        this.grOKGrid.Visibility = Visibility.Collapsed;
                        this.grNGGrid.Visibility = Visibility.Collapsed;
                        return;
                    }

                    this.grOKGrid.Visibility = Visibility.Collapsed;
                    this.grNGGrid.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void txtMLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.V:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        search();
                    }
                    break;
            }

        }


        private void btnUnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cell_id = "";
                string UnHoldreason;
                bool chk_vali = false;

                if (dgCellHistory.ItemsSource == null || dgCellHistory.GetRowCount() == 0)
                {
                    return;
                }

                //해제 사유 필수
                if (cboUnHoldReason.SelectedIndex < 1)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("해제 사유를 선택하세요");
                    ms.AlertWarning("SFU3333"); //선택오류 : 홀드해제사유(필수조건) 콤보를 선택하세요

                    return;
                }

                //판정 값 필수
                if (cboResult1.SelectedIndex < 1)
                {
                    ms.AlertWarning("SFU3815"); //선택오류 : 판정(필수조건) 콤보를 선택하세요

                    return;
                }
                if (txtNote.Text == "")
                {
                    ms.AlertWarning("SFU1404"); //NOTE를 입력 하세요
                    return;
                }

                //해제 작업 시작
                for (int i = 0; i < dgCellHistory.Rows.Count; i++)
                {
                    var chkYn = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK");

                    if (chkYn != null)
                    {
                        if (Convert.ToBoolean(chkYn))
                        {
                            chk_vali = true;
                        }
                    }
                }

                if (!chk_vali)
                {
                    ms.AlertWarning("SFU1651"); //선택된 항목이 없습니다.
                    return;
                }

                setUnHold();

                //Util.AlertInfo("HOLD 해제 완료");
                ms.AlertInfo("SFU1889"); //UNHOLD 완료
                getList();
                txtNote.Text = "";
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        private void setUnHold()
        {
            try
            {
                if (dgCellHistory == null)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("UNHOLD_NOTE", typeof(string));
                INDATA.Columns.Add("UNHOLD_CODE", typeof(string));
                INDATA.Columns.Add("JUDG_FLAG", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["RESNCODE"] = Util.GetCondition(cboUnHoldReason) == "" ? null : cboUnHoldReason.SelectedValue;
                drINDATA["UNHOLD_NOTE"] = txtNote.Text;
                drINDATA["UNHOLD_CODE"] = Util.GetCondition(cboHoldReason) == "" ? null : cboHoldReason.SelectedValue; //P090000T
                drINDATA["JUDG_FLAG"] = cboResult1.SelectedValue;

                INDATA.Rows.Add(drINDATA);

                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));

                int chk_idx = 0;
                int total_row = dgCellHistory.GetRowCount();
                for (int i = 0; i < total_row; i++)
                {
                    if (DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow drINLOT = INLOT.NewRow();
                        drINLOT["LOTID"] = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "LOTID").ToString();
                        INLOT.Rows.Add(drINLOT);

                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }

                dsInput.Tables.Add(INDATA);
                dsInput.Tables.Add(INLOT);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_LOGIS_SAMPLE_LOT_JUDG_RE", "INDATA,INLOT", null, dsInput);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            bool chk_vali = false;
            RadioButton rb = sender as RadioButton;

            bool stExistsOther = false;

            if (rbSample.IsChecked.Value)
            {
                grSampleJudg.Visibility = Visibility.Visible;
                cboResult.SelectedIndex = 0;
                cboResult1.SelectedIndex = 0;
                grReJudg.Visibility = Visibility.Collapsed;
                grNGGrid.Visibility = Visibility.Collapsed;
                grOKGrid.Visibility = Visibility.Collapsed;
            }

            if (rbReInsp.IsChecked.Value)
            {
                grSampleJudg.Visibility = Visibility.Collapsed;
                grReJudg.Visibility = Visibility.Visible;
                cboResult.SelectedIndex = 0;
                cboResult1.SelectedIndex = 0;
            }

            for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
            {
                if (DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    string stsmplinsprslt = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "SMPL_INSP_RSLT").ToString();
                    string stsmpgrinsprslt = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "SMPL_GR_INSP_RSLT").ToString();
                    if (rbSample.IsChecked.Value)
                    {
                        if (!(stsmplinsprslt.Equals("RD")))
                        {
                            stExistsOther = true;
                            DataTableConverter.SetValue(dgCellHistory.Rows[i].DataItem, "CHK", false);
                        }
                    }
                    else if (rbReInsp.IsChecked.Value)
                    {
                        if (stsmplinsprslt.Equals("RD") || stsmpgrinsprslt.Equals("RD"))
                        {
                            stExistsOther = true;
                            DataTableConverter.SetValue(dgCellHistory.Rows[i].DataItem, "CHK", false);
                        }
                    }
                }
            }

            int chkcount = 0;
            for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
            {
                if (DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    chkcount = chkcount + 1;
                }
            }
            if (chkcount == 0)
            {
                grSampleJudg.Visibility = Visibility.Collapsed;
                grReJudg.Visibility = Visibility.Collapsed;
                grNGGrid.Visibility = Visibility.Collapsed;
                grOKGrid.Visibility = Visibility.Collapsed;
            }

            if (stExistsOther)
            {
                Util.MessageInfo("SFU8431");
            }
        }

        private bool chkLot()
        {
            try
            {
                if (dgCellHistory == null)
                {
                    return false;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("IFMODE", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));

                int chk_idx = 0;
                int total_row = dgCellHistory.GetRowCount();
                for (int i = 0; i < total_row; i++)
                {
                    if (DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow drINLOT = INDATA.NewRow();
                        drINLOT["SRCTYPE"] = "OFF";
                        drINLOT["IFMODE"] = "UI";
                        drINLOT["LANGID"] = LoginInfo.LANGID;
                        drINLOT["LOTID"] = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "LOTID").ToString();
                        INDATA.Rows.Add(drINLOT);

                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return false;
                }

                dsInput.Tables.Add(INDATA);

                DataTable dtLOTResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LOGIS_SAMPLE_LOT_CHK", "INDATA", null, INDATA);
                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                rbSample.IsChecked = false;
                grSampleJudg.Visibility = Visibility.Collapsed;
                cboResult.SelectedIndex = 0;
                return false;
            }
        }

        private void txtMLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drLOT = RQSTDT.NewRow();
                drLOT["LOTID"] = txtSearchLotId.Text.Trim();
                RQSTDT.Rows.Add(drLOT);
                DataTable dtLOTResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MAPPINGLOTID", "RQSTDT", "RSLTDT", RQSTDT);

                dgCellHistory.ClearRows();

                if (dtLOTResult.Rows.Count > 0)
                {

                    DataTable dtResult = DataTableConverter.Convert(dgCellHistory.ItemsSource);
                    dtResult = getList(dtLOTResult.Rows[0]["LOTID"].ToString());
                    Util.GridSetData(dgCellHistory, dtResult, FrameOperation, true);
                    txtSearchLotId.Text = "";
                    HiddenLoadingIndicator();
                    //txtSearchLotId.Text = "";
                }
                else
                {
                    Util.MessageValidation("SFU1905");
                }
            }
        }

        private void dgSampleChoice_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox rb = sender as CheckBox;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

            int k = 0;
            int rowlocation = 0;

            for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
            {
                if (DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    k = k + 1;
                    rowlocation = i;
                }
            }

            if (k == 1)
            {
                string stsmplinsprslt = DataTableConverter.GetValue(dgCellHistory.Rows[idx].DataItem, "SMPL_INSP_RSLT").ToString();
                string stsmplgrinsprslt = DataTableConverter.GetValue(dgCellHistory.Rows[idx].DataItem, "SMPL_GR_INSP_RSLT").ToString();

                if (stsmplinsprslt.Equals("RD"))
                {
                    rbSample.IsChecked = true;
                }
                else if (!(stsmplinsprslt.Equals("RD")) && !(stsmplgrinsprslt.Equals("RD")))
                {
                    rbReInsp.IsChecked = true;
                }
                else
                {
                    //DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CHK", false);
                    //Util.MessageInfo("SFU8430");
                    grSampleJudg.Visibility = Visibility.Collapsed;
                    grReJudg.Visibility = Visibility.Collapsed;
                    grNGGrid.Visibility = Visibility.Collapsed;
                    grOKGrid.Visibility = Visibility.Collapsed;
                    rbSample.IsChecked = false;
                    rbReInsp.IsChecked = false;
                }
            }
            else if (k == 0)
            {
                grSampleJudg.Visibility = Visibility.Collapsed;
                grReJudg.Visibility = Visibility.Collapsed;
                grNGGrid.Visibility = Visibility.Collapsed;
                grOKGrid.Visibility = Visibility.Collapsed;
                rbSample.IsChecked = false;
                rbReInsp.IsChecked = false;
            }
            else
            {
                if (rbSample.IsChecked.Value)
                {
                    string stsmplinsprslt = DataTableConverter.GetValue(dgCellHistory.Rows[idx].DataItem, "SMPL_INSP_RSLT").ToString();

                    if (stsmplinsprslt.Equals("RD"))
                    {
                        rbSample.IsChecked = true;
                    }
                    else
                    {
                        Util.MessageInfo("SFU8431");
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CHK", false);
                    }
                }
                else if (rbReInsp.IsChecked.Value)
                {
                    string stsmplinsprslt = DataTableConverter.GetValue(dgCellHistory.Rows[idx].DataItem, "SMPL_INSP_RSLT").ToString();
                    string stsmplgrinsprslt = DataTableConverter.GetValue(dgCellHistory.Rows[idx].DataItem, "SMPL_GR_INSP_RSLT").ToString();
                    if (!stsmplinsprslt.Equals("RD") && !(stsmplgrinsprslt.Equals("RD")))
                    {
                        rbReInsp.IsChecked = true;
                    }
                    else
                    {
                        Util.MessageInfo("SFU8431");
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CHK", false);
                    }
                }
                else
                {

                    for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                        {
                            string stsmplgrinsprslt = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "SMPL_GR_INSP_RSLT").ToString();
                            if (!(stsmplgrinsprslt.Equals("RD")))
                            {
                                Util.MessageInfo("SFU8431");
                                DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CHK", false);
                            }
                        }
                    }
                }
            }
        }

        private void ckGroup_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dtResult = DataTableConverter.Convert(dgCellHistory.ItemsSource);
            dtResult = getList(txtSearchLotId.Text);
            Util.GridSetData(dgCellHistory, dtResult, FrameOperation, true);
        }

        private void ckGroup_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dtResult = DataTableConverter.Convert(dgCellHistory.ItemsSource);
            dtResult = getList(txtSearchLotId.Text);
            Util.GridSetData(dgCellHistory, dtResult, FrameOperation, true);
        }

        private void search()
        {
            // 줄바꿈 자르기
            string[] stringSeparators = new string[] { "\r\n" };
            string sPasteString = Clipboard.GetText();
            string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            string sLotid = null;
            txtSearchLotId.Text = null;
            //없을시
            if (sPasteStrings.Length == 0)
            {
                Util.MessageInfo("SFU1190");
                HiddenLoadingIndicator();
                return;
            }
            //100개 이상시 에러발생
            else if (sPasteStrings.Length > 100)
            {
                Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                HiddenLoadingIndicator();
                return;
            }
            else if (sPasteStrings.Length > 0 && sPasteStrings.Length <= 100)
            {
                ShowLoadingIndicator();
                dgCellHistory.ClearRows();
                DataTable dtResult = DataTableConverter.Convert(dgCellHistory.ItemsSource);
                for (int i = 0; sPasteStrings.Length > i; i++)
                {
                    //컴마로 LOTID 묶기
                    if (string.IsNullOrWhiteSpace(sLotid))
                    {
                        sLotid = sPasteStrings[i];
                    }
                    else
                    {
                        sLotid = (sLotid + "," + sPasteStrings[i]);

                    }
                }
                dtResult = getList(sLotid);
                Util.GridSetData(dgCellHistory, dtResult, FrameOperation, true);
                HiddenLoadingIndicator();
            }
        }

        private void save()
        {
            try
            {
                string bizRuleName = "DA_PRD_UPD_LOGIS_SAMPLE_LOT";

                DataTable isCreateTable = DataTableConverter.Convert(dgCellHistory.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgCellHistory)) return;

                this.dgCellHistory.EndEdit();
                this.dgCellHistory.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SMPL_GR_ID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("SMPL_TRGT_FLAG", typeof(string));

                foreach (object modified in dgCellHistory.GetModifiedItems())
                {
                    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();
                        param["SMPL_GR_ID"] = DataTableConverter.GetValue(modified, "SMPL_GR_ID");
                        param["SMPL_TRGT_FLAG"] = DataTableConverter.GetValue(modified, "SMPL_TRGT_FLAG");
                        param["LOTID"] = DataTableConverter.GetValue(modified, "LOTID");
                        param["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(param);

                        bool check = DataTableConverter.GetValue(modified, "SMPL_GR_INSP_RSLT").Equals("RD");
                        if (!check)
                        {
                            Util.MessageInfo("SFU8191", inDataTable.Rows.Count); //그룹 판정 이전 내역에 한하여 샘플 TARGET 선정 가능 합니다.
                            return;
                        }
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "RQSTDT", null, indataSet);
                Util.MessageInfo("SFU2056", inDataTable.Rows.Count);
                Util.gridClear(dgCellHistory);

                inDataTable = new DataTable();
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

        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    save();
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
                    btnSearch_Click(null, null);
                }
            });
        }

        private void dgCellHistory_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgCellHistory.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgCellHistory.Columns["CHK"]
                 && e.Column != this.dgCellHistory.Columns["SMPL_TRGT_FLAG"]
                 )
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dgCellHistory.BeginNewRow();
                this.dgCellHistory.EndNewRow(true);
            }
            catch (Exception ex)
            {
                FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
            }
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView drv = dgCellHistory.SelectedItem as DataRowView;
                if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                {
                    if (dgCellHistory.SelectedIndex > -1)
                    {
                        dgCellHistory.EndNewRow(true);
                        dgCellHistory.RemoveRow(dgCellHistory.SelectedIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnManualInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK003_029_MANUALPOPUP popup = new PACK003_029_MANUALPOPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[0];
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
