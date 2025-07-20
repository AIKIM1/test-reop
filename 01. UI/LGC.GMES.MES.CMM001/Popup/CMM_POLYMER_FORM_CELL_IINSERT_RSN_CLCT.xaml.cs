/*************************************************************************************
 Created Date : 2018.06.12
      Creator : 정문교
   Decription : DSF 포장모드시 Inbox별 Cell 등록
--------------------------------------------------------------------------------------
 [Change History]
  2021.08.25  공민경 : 불량유형 콤보박스 추가(ESNJ FOX COSMETIC NG 전송 관련)
  2022.06.23  장희만 : C20220524-000581 ESNJ FOX COSMETIC NG 전송 처리 부분 삭제 - 다른 화면에서 FOX 전송 (COM001_370)
      
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CELL_IINSERT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CELL_IINSERT_RSN_CLCT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private bool _load = true;
        private CheckBoxHeaderType _inBoxHeaderType;

        CommonCombo combo = new CommonCombo();

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_CELL_IINSERT_RSN_CLCT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                SetControl();
                _load = false;
            }

            txtInspector.Focus();
        }

        private void InitializeUserControls()
        {
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            _inBoxHeaderType = CheckBoxHeaderType.Zero;

            initCombo();
            GetDefectCode();            
        }
        #endregion

        private void rdoDefect_Checked(object sender, RoutedEventArgs e)
        {
            if (cboResnCode == null) return;

            cboResnCode.IsEnabled = true;
            cboDfctTypeCode.IsEnabled = true;

            if (cboDfctTypeCode.SelectedIndex < 0 || cboDfctTypeCode.SelectedValue.ToString().Equals("SELECT"))
            {
                cboResnCode.SelectedIndex = 0;

                cboResnCode.IsEnabled = false;
            }
        }

        private void rdoDefect_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cboResnCode == null) return;

            cboResnCode.IsEnabled = false;
            cboDfctTypeCode.IsEnabled = false;
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
        #endregion

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
                            dtResult.Columns.Add("NUM");

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

                            for(int inx = 0; inx < dtResult.Rows.Count; inx++)
                            {
                                dtResult.Rows[inx]["NUM"] = inx + 1;
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
                DataTable dt = DataTableConverter.Convert(dgSublot.ItemsSource);

                dt.Select("CHK = 'True'").ToList<DataRow>().ForEach(row => row.Delete());
                dt.AcceptChanges();

                Util.GridSetData(dgSublot, dt, null);

                txtCellQty.Text = dgSublot.GetRowCount().ToString();
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

        #region 저장
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

        #region Mehod

        private void initCombo()
        {        
            //불량유형
            string[] sFilter = { "", "FOX_DFCT_TYPE_CODE" };
            combo.SetCombo(cboDfctTypeCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODES");
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
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID, "DEFECT_LOT" };
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
                newRow["PROCID"] = _procID;
                newRow["EQPTID"] = _eqptID;
                newRow["ACTUSER"] = txtInspectorName.Tag.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(dgSublot.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    newRow = inSublot.NewRow();
                    newRow["SUBLOTID"] = row["CELLID"];
                    newRow["ACTID"] = string.IsNullOrWhiteSpace(row["RESNCODE"].ToString()) ? "GOOD_SUBLOT" : "DEFECT_SUBLOT"; //2022.06.20 FOX 전송 안함 구분하기 위해 변경 _BLACK 추가
                    newRow["RESNCODE"] = string.IsNullOrWhiteSpace(row["RESNCODE"].ToString()) ? "G" : row["RESNCODE"];
                    inSublot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_INS_SUBLOT_RSN_NEW_CLCT", "INDATA,INSUBLOT", null, (bizResult, bizException) => // 2022.06.20 BR_PRD_INS_SUBLOT_RSN_CLCT 대신 BR_PRD_INS_SUBLOT_RSN_NEW_CLCT로 변경 FOX전송 안함.
                {
                    try                                
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            Util.gridClear(dgSublot);
                            return;
                        }

                        //Util.gridClear(dgSublot);

                        // 저장 되었습니다
                        Util.MessageInfo("SFU3532");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.ShowExceptionMessages(ex);
                      
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

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

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }


        #endregion


    }
}