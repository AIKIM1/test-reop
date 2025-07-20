/*************************************************************************************
 Created Date : 2023.12.19
      Creator : 김대현
   Decription : 설비 Loss 수정 이력
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.19 김대현 E20231208-001776 설비 Loss 등록, 수정 화면 통합
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_014_Tab_Req_Hist.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_014_Tab_Req_Hist : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        int iEqptCnt;

        bool bPack;
        bool bMPPD = false; // Modifiable person on the previous day : Pack 전용 전일 수정 가능자 (신규 권한)
        bool bForm;
        bool isOnlyRemarkSave = false;  // CSR : C20220512-000432
        bool isTotalSave = false;       // CSR : C20220512-000432

        string sMainEqptID;
        string sSearchDay = "";

        string RunSplit; //동, 공정에 따라 RUN상태를 Split할 수 있는지 구분
        string _grid_eqpt = string.Empty;
        string _grid_area = string.Empty;
        string _grid_proc = string.Empty;
        string _grid_eqsg = string.Empty;
        string _grid_shit = string.Empty;

        List<string> liProcId;

        DataTable dtMainList = new DataTable();
        DataTable AreaTime;
        DataTable dtShift;
        DataTable dtRemarkMandatory;
        DataTable dtQA;                 // CSR : C20220512-000432
        Hashtable hash_loss_color = new Hashtable();
        Hashtable org_set;

        DataSet dsEqptTimeList = null;

        string strAttr1 = string.Empty;
        string strAttr2 = string.Empty;
        string sNowDay = string.Empty;
        bool bUseEqptLossAppr = false; // CSR : E20230420-001240, 설비 LOSS 수정 화면 추가에 따른 Validation 추가

        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre1 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        string MachineEqptChk = string.Empty;   //Machine 설비 Loss 수정 가능여부 Flag :    2023.03.16 오화백
        string MachineEqptChkHist = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public COM001_014_Tab_Req_Hist()
        {
            InitializeComponent();

            InitCombo();

            if (bPack)
            {
                GetPackAuth();
            }
        }

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();
            cboEquipmentSegmentHist1.ApplyTemplate();
            cboEquipmentHist.ApplyTemplate();

            if (string.IsNullOrWhiteSpace(LoginInfo.CFG_AREA_ID) || LoginInfo.CFG_AREA_ID.Length < 1 || !LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P"))
            {
                bPack = false;

                //2023.03.07 활성화 구분 추가
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                    bForm = true;
                else
                    bForm = false;

                // 설비 Loss 수정 이력
                SetAreaCombo(cboAreaHist);

                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    cboEquipmentSegmentHist.Visibility = Visibility.Visible;
                    cboEquipmentSegmentHist1.Visibility = Visibility.Collapsed;

                    SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist);
                    SetProcessHistComboFCS(cboProcessHist);
                }
                else
                {
                    cboEquipmentSegmentHist.Visibility = Visibility.Collapsed;
                    cboEquipmentSegmentHist1.Visibility = Visibility.Visible;

                    SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist1);
                    SetProcessHistCombo(cboProcessHist);
                }

                SetEquipmentHistCombo(cboEquipmentHist);
                SetShiftHistCombo(cboShiftHist);

                cboAreaHist.SelectedValueChanged += cboAreaHist_SelectedValueChanged;
                cboEquipmentSegmentHist.SelectedValueChanged += cboEquipmentSegmentHist_SelectedValueChanged;
                cboProcessHist.SelectedValueChanged += cboProcessHist_SelectedValueChanged;
                cboEquipmentHist.SelectionChanged += cboEquipmentHist_SelectionChanged;
            }
            else
            {
                // Pack인 경우
                bPack = true;
                bForm = false;

                // 설비 Loss 수정 이력
                SetAreaCombo(cboAreaHist);
                SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist1);
                SetProcessHistCombo(cboProcessHist);
                SetEquipmentHistCombo(cboEquipmentHist);
                SetShiftHistCombo(cboShiftHist);

                cboEquipmentSegmentHist.Visibility = Visibility.Collapsed;
                cboEquipmentSegmentHist1.Visibility = Visibility.Visible;

                cboAreaHist.SelectedValueChanged += cboAreaHist_SelectedValueChanged;
                cboProcessHist.SelectedValueChanged += cboProcessHist_SelectedValueChanged;
                cboEquipmentHist.SelectionChanged += cboEquipmentHist_SelectionChanged;
            }
        }
        #endregion

        #region SetCombeBox
        private void SetAreaCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID))
            {
                cbo.SelectedIndex = 0;
            }
            else
            {
                cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
        }

        private void SetEquipmentSegmentHistCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";

                inTable.Columns.Add("INCLUDE_GROUP", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaHist.SelectedValue;
                dr["INCLUDE_GROUP"] = "AC";

                inTable.Rows.Add(dr);
            }
            else
            {
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaHist.SelectedValue;

                inTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQSG_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
        }

        private void SetEquipmentSegmentHistCombo(MultiSelectionBox cbo)
        {
            cbo.ItemsSource = null;

            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));


            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboAreaHist.SelectedValue;

            inTable.Rows.Add(dr);


            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();
        }

        private void SetProcessHistComboFCS(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegmentHist.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_PROC_ID;

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }

            MachineEqptHist_Loss_Modify_Chk(cboProcessHist.SelectedValue.GetString());
        }

        private void SetProcessHistCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string str = string.Empty;
            str = SelectEquipmentSegmentHist1();

            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = str;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            if (CommonVerify.HasTableRow(dtResult))
            {
                cbo.ItemsSource = dtResult.Copy().AsDataView();
            }

            if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }

            MachineEqptHist_Loss_Modify_Chk(cboProcessHist.SelectedValue.GetString());
        }

        private void SetEquipmentHistCombo(MultiSelectionBox cbo)
        {
            cbo.ItemsSource = null;
            //cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_MULTI_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                dr["EQSGID"] = cboEquipmentSegmentHist.SelectedValue;
            }
            else
            {
                dr["EQSGID"] = SelectEquipmentSegmentHist1();
            }
            dr["PROCID"] = cboProcessHist.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            if (CommonVerify.HasTableRow(dtResult))
            {
                cbo.ItemsSource = DataTableConverter.Convert(dtResult);
            }
        }

        private void SetShiftHistCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = string.IsNullOrEmpty(cboAreaHist.SelectedValue.GetString()) ? null : cboAreaHist.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegmentHist.SelectedValue.GetString()) ? null : cboEquipmentSegmentHist.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcessHist.SelectedValue.GetString()) ? null : cboProcessHist.SelectedValue.GetString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        private void SetMachineEqptHistCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_MACHINE_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(cboAreaHist.SelectedValue.GetString()) ? null : cboAreaHist.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegmentHist.SelectedValue.GetString()) ? null : cboEquipmentSegmentHist.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcessHist.SelectedValue.GetString()) ? null : cboProcessHist.SelectedValue.GetString();
            dr["EQPTID"] = SelectEquipment();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }
        #endregion

        #region Method
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private bool SearchDatechk()
        {
            if ((ldpDateTo.SelectedDateTime - ldpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return true;
            }
            return false;
        }

        private void CancelInputRequest()
        {
            try
            {
                if (!CheckCancelValidation()) return;

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("RQSTDT");
                inTable.Columns.Add("APPR_STAT", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("APPR_SEQNO", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("STRT_DTTM", typeof(string));
                inTable.Columns.Add("END_DTTM", typeof(string));
                inTable.Columns.Add("WRK_DATE", typeof(string));
                inTable.Columns.Add("LOSS_SEQNO", typeof(string));
                inTable.Columns.Add("REQ_SEQNO", typeof(string));

                for (int i = 0; i < dgDetailHist.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgDetailHist, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["APPR_STAT"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "APPR_STAT"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["APPR_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "APPR_SEQNO"));
                    newRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "EQPTID"));
                    newRow["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "WRK_DATE"));
                    newRow["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "LOSS_SEQNO"));
                    newRow["REQ_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "REQ_SEQNO"));

                    inTable.Rows.Add(newRow);
                }

                // 취소하시겠습니까?
                Util.MessageConfirm("SFU4616", sResult =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_APPR_CANCEL", "RQSTDT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                // 취소되었습니다.
                                Util.MessageInfo("SFU1937");

                                // 재조회
                                GetEqptLossChangeApprHistList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region Data
        private void GetPackAuth()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("AUTHID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "PACK_LOSS_ENGR_CWA";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count != 0)
                {
                    bMPPD = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string SelectEquipmentSegmentHist1()
        {
            string sEqsgID = string.Empty;
            for (int i = 0; i < cboEquipmentSegmentHist1.SelectedItems.Count; i++)
            {
                if (i != cboEquipmentSegmentHist1.SelectedItems.Count - 1)
                {
                    sEqsgID += cboEquipmentSegmentHist1.SelectedItems[i] + ",";
                }
                else
                {
                    sEqsgID += cboEquipmentSegmentHist1.SelectedItems[i];
                }
            }

            return sEqsgID;
        }

        private string SelectEquipment()
        {
            string sEqptID = string.Empty;

            for (int i = 0; i < cboEquipmentHist.SelectedItems.Count; i++)
            {
                if (i < cboEquipmentHist.SelectedItems.Count - 1)
                {
                    sEqptID += Convert.ToString(cboEquipmentHist.SelectedItems[i]) + ",";
                }
                else
                {
                    sEqptID += Convert.ToString(cboEquipmentHist.SelectedItems[i]);
                }
            }

            return sEqptID;
        }

        private void GetEqptLossChangeApprHistList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable RSLTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROM_WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = SelectEquipment();
                dr["FROM_WRK_DATE"] = Util.GetCondition(ldpDateFrom);
                dr["TO_WRK_DATE"] = Util.GetCondition(ldpDateTo);

                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_APPROVAL_TARGET_MULTI_LIST", "RQSTDT", "RSLTDT", RQSTDT);
                dgDetailHist.MergingCells -= dgDetailHist_MergingCells;
                Util.GridSetData(dgDetailHist, RSLTDT, FrameOperation, true);
                dgDetailHist.MergingCells += dgDetailHist_MergingCells;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Events

        #region Buttons
        public void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SearchDatechk())
                {
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                GetEqptLossChangeApprHistList();
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelInputRequest();
        }
        #endregion

        #region ComboBox
        private void cboAreaHist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (bPack) return;

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist);
            }
            else
            {
                SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist1);
            }
        }

        private void cboEquipmentSegmentHist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                SetProcessHistComboFCS(cboProcessHist);
            }
            else
            {
                SetProcessHistCombo(cboProcessHist);
            }
            SetShiftHistCombo(cboShiftHist);
        }

        private void cboProcessHist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (bPack) return;

            SetEquipmentHistCombo(cboEquipmentHist);
            MachineEqptHist_Loss_Modify_Chk(cboProcessHist.SelectedValue.GetString());
        }

        private void cboEquipmentHist_SelectionChanged(object sender, EventArgs e)
        {
            if (bPack) return;

            SetShiftHistCombo(cboShiftHist);
        }

        private void cboEquipmentSegmentHist1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                cboProcessHist.ItemsSource = null;
                cboProcessHist.Items.Clear();

                string str = string.Empty;

                str = SelectEquipmentSegmentHist1();

                if (string.IsNullOrEmpty(str)) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = str;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcessHist.DisplayMemberPath = "CBO_NAME";
                cboProcessHist.SelectedValuePath = "CBO_CODE";

                if (CommonVerify.HasTableRow(dtResult))
                {
                    cboProcessHist.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
                }

                if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
                {
                    cboProcessHist.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cboProcessHist.SelectedIndex < 0)
                    {
                        cboProcessHist.SelectedIndex = 0;
                    }
                }
                else
                {
                    cboProcessHist.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region DataGrid
        private void dgDetailHist_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string appr_stat = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "APPR_STAT"));
                    if (appr_stat.Equals("R"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightCoral);
                    }
                    if (appr_stat.Equals("A"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                }
            }));
        }

        private void dgDetailHist_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.Rows.Count == 0) return;

            List<System.Data.DataRow> list = DataTableConverter.Convert(dg.ItemsSource).Select().ToList();
            List<int> arr = list.GroupBy(c => c["REQ_SEQNO"]).Select(group => group.Count()).ToList();

            int p = 0;
            for (int j = 0; j < arr.Count; j++)
            {
                for (int i = 0; i < dg.Columns.Count; i++)
                {
                    if (dg.Columns[i].Name.Equals("CHK") 
                        || dg.Columns[i].Name.Equals("EQPTNAME")
                        || dg.Columns[i].Name.Equals("APPR_REQ_USERNAME")
                        || dg.Columns[i].Name.Equals("APPR_REQ_DTTM")
                        || dg.Columns[i].Name.Equals("APPR_USERNAME")
                        || dg.Columns[i].Name.Equals("APPR_DTTM")
                        || dg.Columns[i].Name.Equals("APPR_STAT_NAME"))
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(p, i), dg.GetCell((p + arr[j] - 1), i)));
                    }
                }
                p += arr[j];
            }
        }

        #endregion

        #endregion

        #region Validation
        private void MachineEqptHist_Loss_Modify_Chk(string ProcId)
        {
            if (string.IsNullOrEmpty(ProcId)) return;

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EQPTLOSS_MACHINE_EQPT_MODIFY_PROCESS";
                dr["COM_CODE"] = ProcId;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    MachineEqptChkHist = "Y";
                }
                else
                {
                    MachineEqptChkHist = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CheckCancelValidation()
        {
            DataRow[] drInfo = Util.gridGetChecked(ref dgDetailHist, "CHK");

            if (drInfo.Length == 0)
            {
                Util.MessageValidation("SFU1636");  // 선택된 대상이 없습니다.
                return false;
            }
            else
            {
                if (CheckManagerAuth())
                    return true;

                foreach (DataRow dr in drInfo)
                {
                    if (dr["APPR_REQ_USERID"].ToString() != LoginInfo.USERID)
                    {
                        Util.MessageValidation("SFU5184");  // 요청자만 요청 취소가 가능합니다.
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CheckManagerAuth()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows?.Count <= 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion
    }
}
