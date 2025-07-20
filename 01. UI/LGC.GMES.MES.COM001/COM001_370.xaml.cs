/*************************************************************************************
 Created Date : 2022.06.14
      Creator : 장희만
   Decription : 불량 FOX Interface
--------------------------------------------------------------------------------------
 [Change History]
  2022.06.14   C20220524-000581  최초개발, NJ NT Cosmetic data transfer optimization 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_368.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_370 : UserControl, IWorkArea
    {

        #region Declaration & Constructor

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private bool _load = true;
        private CheckBoxHeaderType _inBoxHeaderType;

        CommonCombo combo = new CommonCombo();

        #endregion


        private string _UserID = string.Empty; //직접 실행하는 USerID
        private string _authorityGroup = string.Empty;

        private enum TYPE
        {
            MAPPING = 0,
            EMPTY = 1
        }

        public COM001_370()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }
        public MessageBoxResult DialogResult { get; private set; }

        private void TabMapping_GotFocus(object sender, RoutedEventArgs e)
        {
            TabMapping.GotFocus -= TabMapping_GotFocus;
            //TabEmpty.GotFocus += TabEmpty_GotFocus;
            //ClearAll(txtCSTID, txtLOTID, dgMapping);
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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
            if (_load)
            {
                SetControl();
                _load = false;
            }

            txtInspector.Focus();

            dtpDateFrom.SelectedDateTime = DateTime.Today.AddDays(-10);
            dtpDateTo.SelectedDateTime = DateTime.Today;

        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            ////_procID = tmps[0] as string;
            ////_eqptID = tmps[2] as string;

            ////txtProcess.Text = tmps[1] as string;
            ////txtEquipment.Text = tmps[3] as string;

            _inBoxHeaderType = CheckBoxHeaderType.Zero;

            initCombo();
            GetDefectCode();

        }
        private void initCombo()
        {
            //불량유형
            string[] sFilter = { "", "FOX_DFCT_TRANS_CODE" };
            combo.SetCombo(cboDfctTypeCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODES");

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            C1ComboBox[] cboAreaChildquery = { cboEquipmentSegment };
            _combo.SetCombo(cboAreaquery, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);


            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            C1ComboBox[] cboEquipmentSegmentParentquery = { cboAreaquery };
            C1ComboBox[] cboEquipmentSegmentChildquery = { cboProcessquery, cboEquipment };
            _combo.SetCombo(cboEquipmentSegmentquery, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChildquery, cbParent: cboEquipmentSegmentParentquery);


            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            C1ComboBox[] cboProcessParentquery = { cboEquipmentSegmentquery };
            C1ComboBox[] cboProcessChildquery = { cboEquipmentquery };
            _combo.SetCombo(cboProcessquery, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChildquery, cbParent: cboProcessParentquery);



            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            C1ComboBox[] cboEquipmentParentquery = { cboEquipmentSegmentquery, cboProcessquery };
            _combo.SetCombo(cboEquipmentquery, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParentquery);


            DataTable dtNest = new DataTable();
            dtNest.Columns.Add("CBO_NAME", typeof(string));
            dtNest.Columns.Add("CBO_CODE", typeof(string));

            for (int i = 1; i < 9; i++)
            {
                DataRow dr = dtNest.NewRow();
                dr["CBO_NAME"] = "Nest" + i.ToString();
                dr["CBO_CODE"] = @"ko-KR\Nest" + i.ToString();

                dtNest.Rows.Add(dr);
            }

        }

        /// <summary>
        /// 불량 코드 조회 
        /// </summary>
        private void GetDefectCode()
        {
            try
            {
                const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT_INFO_PC_SUBLOT";
                string[] arrColumn = { "LANGID", "AREAID", "PROCID", "ACTID" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboProcess.SelectedValue.ToString(), "DEFECT_LOT" };
                string selectedValueText = cboResnCode.SelectedValuePath;
                string displayMemberText = cboResnCode.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cboResnCode, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private void rdoDefect_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cboResnCode == null) return;

            ////cboResnCode.IsEnabled = false;
            ////cboDfctTypeCode.IsEnabled = false;
        }

        #region 검사자
        private void txtInspector_GotFocus(object sender, RoutedEventArgs e)
        {
            dgInspectorSelect.Visibility = Visibility.Collapsed;
        }

        private void txtInspector_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                GetInspector();
            }
        }

        private void dgInspector_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtInspector.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtInspectorName.Tag = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtInspectorName.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());

            dgInspectorSelect.Visibility = Visibility.Collapsed;

            txtCellID.Focus();
        }

        #region Cell
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtCellID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        txtCellID.Text = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(txtCellID.Text))
                            txtCellID_KeyDown(txtCellID, null);

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key == System.Windows.Input.Key.Enter)
                {
                    if ((bool)rdoDefect.IsChecked)
                    {
                        if (cboResnCode.SelectedIndex < 0 || cboResnCode.SelectedValue.ToString().Equals("SELECT"))
                        {
                            // 선택된 불량 정보가 없습니다.
                            Util.MessageValidation("SFU4382");
                            return;
                        }
                    }

                    string sCellID = txtCellID.Text.Trim();



                    if (dgSublot.GetRowCount() > 0)
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgSublot.ItemsSource);
                        DataRow[] drList = dtInfo.Select("CELLID = '" + sCellID + "'");

                        if (drList.Length > 0)
                        {
                            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellID.Focus();
                                    txtCellID.Text = string.Empty;
                                }
                            });

                            txtCellID.Text = string.Empty;
                            return;
                        }
                    }

                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("SUBLOTID");

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_RSN_CLCT", "INDATA", "OUTDATA", RQSTDT);

                    if (dtResult != null)
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            dtResult.Columns.Add("RESNCODE");
                            dtResult.Columns.Add("RESNNAME");
                            if ((bool)rdoDefect.IsChecked)
                            {
                                dtResult.Rows[0]["RESNCODE"] = cboResnCode.SelectedValue.ToString();
                                dtResult.Rows[0]["RESNNAME"] = cboResnCode.Text;
                            }

                            if (dgSublot.GetRowCount() > 0)
                            {
                                DataTable dt = DataTableConverter.Convert(dgSublot.ItemsSource);
                                DataRow[] drList = dt.Select("CELLID = '" + dtResult.Rows[0]["CELLID"] + "'");

                                if (drList.Length > 0)
                                {
                                    // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                                    ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            txtCellID.Focus();
                                            txtCellID.Text = string.Empty;
                                        }
                                    });

                                    txtCellID.Text = string.Empty;
                                    return;
                                }

                                DataTable dtInfo = DataTableConverter.Convert(dgSublot.ItemsSource);
                                dtResult.Merge(dtInfo);
                            }

                            Util.GridSetData(dgSublot, dtResult, null);
                            txtCellQty.Text = dgSublot.GetRowCount().ToString();
                        }
                    }

                    txtCellID.Text = string.Empty;
                    txtCellID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageExceptionNoEnter(ex, msgResult =>
                {
                    if (msgResult == MessageBoxResult.OK)
                    {
                        txtCellID.Text = string.Empty;
                        txtCellID.Focus();
                    }
                });
            }
            finally
            {
            }
        }
        #endregion

        #region Cell 삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSublot.Rows.Count > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgSublot.ItemsSource);
                    dt.Select("CHK = 'True'").ToList<DataRow>().ForEach(row => row.Delete());
                    dt.AcceptChanges();

                    Util.GridSetData(dgSublot, dt, null);
                    txtCellQty.Text = dgSublot.GetRowCount().ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Grid Header Check
        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgSublot;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }
        #endregion

        #region Cell 저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            // 저장 하시겠습니까?
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveSublot();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion


        /// <summary>
        /// 작업자 조회
        /// </summary>
        private void GetInspector()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERNAME", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERNAME"] = txtInspector.Text;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", inTable);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    // 사용자 정보가 없습니다.
                    Util.MessageValidation("SFU1592");
                }
                else if (dtResult.Rows.Count == 1)
                {
                    txtInspector.Text = dtResult.Rows[0]["USERID"].ToString();
                    txtInspectorName.Tag = dtResult.Rows[0]["USERID"].ToString();
                    txtInspectorName.Text = dtResult.Rows[0]["USERNAME"].ToString();

                    txtCellID.Focus();
                }
                else
                {
                    dgInspectorSelect.Visibility = Visibility.Visible;

                    Util.GridSetData(dgInspectorSelect, dtResult, null);
                    this.Focusable = true;
                    this.Focus();
                    this.Focusable = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Sublot(cell) 저장
        /// </summary>
        private void SaveSublot()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ACTUSER", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inSublot = inDataSet.Tables.Add("INSUBLOT");
                inSublot.Columns.Add("SUBLOTID", typeof(string));
                inSublot.Columns.Add("ACTID", typeof(string));
                inSublot.Columns.Add("RESNCODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["ACTUSER"] = txtInspectorName.Tag.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(dgSublot.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    newRow = inSublot.NewRow();
                    newRow["SUBLOTID"] = row["CELLID"];
                    newRow["ACTID"] = string.IsNullOrWhiteSpace(row["RESNCODE"].ToString()) ? "GOOD_SUBLOT" : "DEFECT_SUBLOT_COSMETIC"; //Cosmetic NG와 구분하기 위해 ACTID 변경
                    newRow["RESNCODE"] = string.IsNullOrWhiteSpace(row["RESNCODE"].ToString()) ? "G" : row["RESNCODE"];
                    inSublot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_INS_SUBLOT_RSN_CLCT", "INDATA,INSUBLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.gridClear(dgSublot);

                        // 저장 되었습니다
                        Util.MessageInfo("SFU3532");

                        this.DialogResult = MessageBoxResult.OK;
                        Util.gridClear(dgSublot);
                        HiddenLoadingIndicator();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        #region [Func]

        private bool ValidationSave()
        {
            if (dgSublot.ItemsSource == null)
            {
                // SFU3552 저장 할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                return false;
            }

            if (txtInspectorName.Tag == null || string.IsNullOrWhiteSpace(txtInspectorName.Tag.ToString()))
            {
                // 검사자를 입력해주세요
                Util.MessageValidation("SFU1452");
                return false;
            }

            return true;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        #endregion

        private void rdoDefect_Checked(object sender, RoutedEventArgs e)
        {
            //if (cboResnCode == null) return;

            //cboResnCode.IsEnabled = true;
            //cboDfctTypeCode.IsEnabled = true;

            //if (cboDfctTypeCode.SelectedIndex < 0 || cboDfctTypeCode.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    cboResnCode.SelectedIndex = 0;

            //    cboResnCode.IsEnabled = false;
            //}
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                //불량유형
                string[] sFilter = { "", "FOX_DFCT_TRANS_CODE" };
                combo.SetCombo(cboDfctTypeCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODES");

                //불량항목 셋팅
                GetDefectCode();
            }


        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //Cosmetic NG Fox 전송 리스트 조회
            GetCosmeticNGList();
        }

        public void GetCosmeticNGList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                //dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentquery, bAllNull: true);

                if (cboEquipmentquery.SelectedValue.ToString() != "SELECT")
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipmentquery, bAllNull: true);
                }

                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                dr["TODATE"] = Util.GetCondition(dtpDateTo);
                dr["SUBLOTID"] = string.IsNullOrWhiteSpace(txtCellIDSearch.Text) ? null : txtCellIDSearch.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FOX_SUBLOT_RSN_CLCT", "RQSTDT", "OUTDATA", dtRqst);

                Util.gridClear(dgCosmeticList);
                dgCosmeticList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgCosmeticList, dtRslt, FrameOperation, false);

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

        private void txtCellIDSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtCellIDSearch.Text.ToString().Trim()))
                    {
                        // CELLID를 스캔 또는 입력하세요.
                        Util.MessageInfo("SFU1323");
                        return;
                    }

                    ShowLoadingIndicator();

                    GetCellList(txtCellIDSearch.Text.ToString().Trim());
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtCellIDSearch.Text = string.Empty;
                    txtCellIDSearch.SelectAll();
                    txtCellIDSearch.Focus();

                    HiddenLoadingIndicator();
                }
            }
        }
        private void GetCellList(string pCellID)
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["SUBLOTID"] = pCellID;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FOX_SUBLOT_RSN_CLCT", "RQSTDT", "OUTDATA", inTable);

                if (dtRslt.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgCosmeticList.ItemsSource);
                    dtSource.Merge(dtRslt);
                    Util.gridClear(dgCosmeticList);
                    Util.GridSetData(dgCosmeticList, dtSource, FrameOperation, false);
                }

                ////if (dgCosmeticList.Rows.Count <= 0)
                ////{
                ////    Util.MessageInfo("SFU2951");    //조회결과가 없습니다.
                ////}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void txtCellIDSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Length > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    ShowLoadingIndicator();

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;

                        }

                        bool isExist = false;

                        if (dgCosmeticList.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgCosmeticList.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgCosmeticList.Rows[idx].DataItem, "SUBLOTID").ToString() == sPasteStrings[i])
                                {
                                    isExist = true;
                                    break;
                                }
                            }
                        }

                        if (isExist == false)
                        {
                            GetCellList(sPasteStrings[i]);
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        private void dgCosmeticList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgCosmeticList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

            }));
        }

    }


}
