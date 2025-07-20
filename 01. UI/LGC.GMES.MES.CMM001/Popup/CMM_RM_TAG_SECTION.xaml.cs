/*************************************************************************************
 Created Date : 2024.05.07
      Creator : 신광희
   Decription : ROLLMAP TAG SECTION 등록 수정 삭제 (CMM_ROLLMAP_PSTN_UPD 참고) 변경된 BizRule 적용 필요
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.07  : Initial Created.
  2024.08.28  조성근 : [E20240827-000372] 롤프레스 롤맵 수불 적용
  2025.03.21  조성근   [E20250324-000244] 롤프레스 롤맵 수불 개선건
  2025.03.21  조성근   [E20250325-000705] 롤프레스 롤맵 아웃피드 스크랩 추가
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_RM_TAG_SECTION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string popSTRT_PSTN = string.Empty;
        private string popEND_PSTN = string.Empty;

        private string _runLotId = string.Empty;
        private string _startPosition = string.Empty;
        private string _endPosition = string.Empty;
        private string _processCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _runWipSeq = string.Empty;
        private int _collectSeq = 0;
        private int _updatecollectSeq = 0;
        private Boolean updateFlag = false;

        private string _collectType = string.Empty;
        private bool _isLoaded;
        private bool _isNew;
        private string _lotId = string.Empty;
        private string _wipseq = string.Empty;
        private decimal _limitCutLength = 0;
        private Visibility _saveButtonVisibility;
        private string _Relative = "Y";

        public bool IsUpdated { get; set; }

        public CMM_RM_TAG_SECTION()
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

        #endregion

        #region Initialize


        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitializeCombo()
        {
            SetComboBox(cboBadSection);
            SetComboBox(cboBadtype);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitializeControl()
        {
            txtStartPosition.Value = Math.Round(popSTRT_PSTN.GetDouble(), 1);
            txtEndPosition.Value = Math.Round(popEND_PSTN.GetDouble(), 1);
        }
        private bool SelectRollMapLot()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_RM_RPT_LOT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = _runLotId;
                dr["WIPSEQ"] = _runWipSeq;
                inTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dt))
                {
                    if (dt.Rows[0]["ROLLMAP_LOT_YN"].Equals("Y"))
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)
            {
                _runLotId = Util.NVC(parameters[0]);
                _startPosition = Util.NVC(parameters[1]);
                _endPosition = Util.NVC(parameters[2]);
                _processCode = Util.NVC(parameters[3]);
                _equipmentCode = Util.NVC(parameters[4]);
                _runWipSeq = Util.NVC(parameters[5]);
                _collectSeq = Util.NVC_Int(parameters[6]);
                _collectType = Util.NVC(parameters[7]);
                _isNew = (bool)parameters[8];
                _limitCutLength = Util.NVC_Decimal(parameters[9]);
                _saveButtonVisibility = (Visibility)parameters[10];
                if (parameters.Length > 11) _Relative = Util.NVC(parameters[11]);    //조성근 2025.04.09 ScrapSection 적용

                _updatecollectSeq = _collectSeq;
                popSTRT_PSTN = _startPosition;
                popEND_PSTN = _endPosition;

                btnSave.Visibility = _saveButtonVisibility;
                btnDel.Visibility = _saveButtonVisibility;
                if (SelectRollMapLot() == true)
                {
                    btnDel.Visibility = Visibility.Collapsed;
                }

                if (_saveButtonVisibility == Visibility.Collapsed)
                {
                    txtStartPosition.IsReadOnly = true;
                    txtWidth.IsReadOnly = true;
                    txtEndPosition.IsReadOnly = true;
                    chkStartPosition.IsEnabled = false;
                    chkEndPosition.IsEnabled = false;
                    cboBadSection.IsEnabled = false;
                    cboBadtype.IsEnabled = false;
                }

                Header = ObjectDic.Instance.GetObjectName("ROLLMAP_TAG_SECTION_SAVE");

                if (_Relative == "N" && _isNew == true)
                {
                    Header = Header + " * " + ObjectDic.Instance.GetObjectName("절대좌표") + " *";
                }
            }

            chkStartPosition.Checked += chkStartPosition_Checked;
            chkStartPosition.Unchecked += chkStartPosition_UnChecked;
            chkEndPosition.Checked += chkEndPosition_Checked;
            chkEndPosition.Unchecked += chkEndPosition_UnChecked;

            //if (string.IsNullOrEmpty(_STRT_PSTN) && string.IsNullOrEmpty(_END_PSTN))
            if (_isNew)
            {
                cboBadSection.SelectedIndexChanged -= cboBadSection_SelectedIndexChanged;
                InitializeCombo();


                if (!string.IsNullOrEmpty(_startPosition) && !string.IsNullOrEmpty(_endPosition))
                {
                    txtStartPosition.Value = Math.Round(popSTRT_PSTN.GetDouble(), 1);
                    txtEndPosition.Value = Math.Round(popEND_PSTN.GetDouble(), 1);
                    txtWidth.Value = Math.Round(popEND_PSTN.GetDouble(), 1) - Math.Round(popSTRT_PSTN.GetDouble(), 1);
                }
                else
                {
                    txtStartPosition.Value = 0;
                    txtEndPosition.Value = 0;
                    txtWidth.Value = 0;
                }

                InitClear();
                cboBadSection.SelectedIndexChanged += cboBadSection_SelectedIndexChanged;
            }
            else
            {
                InitializeCombo();
                InitializeControl();
                CheckReadonly();
            }
            SelectProductQty();
            //txtRWQty.Value = _limitCutLength.GetDouble();
            _isLoaded = true;

        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1230", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveDefectForRollMap("D");
                    }
                });
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
                if (Util.NVC_Decimal(txtStartPosition.Value) > Util.NVC_Decimal(txtEndPosition.Value) || Util.NVC_Decimal(txtStartPosition.Value) < 0
                    || (_limitCutLength != 0 && (Util.NVC_Decimal(txtStartPosition.Value) > _limitCutLength || Util.NVC_Decimal(txtEndPosition.Value) > _limitCutLength)))
                {
                    Util.MessageInfo("SFU8116", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (chkStartPosition.IsChecked != null && (bool)chkStartPosition.IsChecked)
                            {
                                txtEndPosition.Focus();
                            }

                            if (chkEndPosition.IsChecked != null && (bool)chkEndPosition.IsChecked)
                            {
                                txtStartPosition.Focus();
                            }

                            if (chkStartPosition.IsChecked != null &&
                                (chkEndPosition.IsChecked != null &&
                                 ((bool)chkEndPosition.IsChecked == false &&
                                  (bool)chkStartPosition.IsChecked == false)))
                            {
                                txtEndPosition.Focus();
                            }
                        }
                    }, ObjectDic.Instance.GetObjectName("위치"));
                    return;
                }

                if (string.Equals(_processCode, Process.COATING) || string.Equals(_processCode, Process.ROLL_PRESSING))
                {
                    // 기존 좌표랑 하나라도 동일하면 업데이트 아닐시 신규 등록
                    if (updateFlag == true)
                    {
                        //신규
                        SaveDefectForRollMap("N");
                    }
                    else if (updateFlag == false)
                    {
                        //수정
                        SaveDefectForRollMap("U");
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefectForRollMap(string sFlag)
        {
            try
            {
                if (_processCode == Process.ROLL_PRESSING)
                {
                    SaveDefectForRollMap_RP(sFlag);
                    return;
                }

                const string bizRuleName = "BR_PRD_REG_RM_RPT_TAG_SECTION";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USER", typeof(string));
                inTable.Columns.Add("FORCE_FLAG", typeof(string)); // Y 이면 다음 공정 투입 여부 체크 안함

                DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
                inRollmapTable.Columns.Add("LOTID", typeof(string));
                inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
                inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
                inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
                inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
                inRollmapTable.Columns.Add("ACTID", typeof(string));
                inRollmapTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
                inRollmapTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
                inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
                inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));

                inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
                inRollmapTable.Columns.Add("TOP_RESNCODE", typeof(string));    //TOP 활동유형코드
                inRollmapTable.Columns.Add("BACK_RESNCODE", typeof(string));   //BACK 활동유형코드

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["USER"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow newRollRow = inRollmapTable.NewRow();
                newRollRow["LOTID"] = string.IsNullOrEmpty(_lotId) ? _runLotId : _lotId;
                newRollRow["WIPSEQ"] = string.IsNullOrEmpty(_wipseq) ? _runWipSeq : _wipseq;
                newRollRow["EQPT_MEASR_PSTN_ID"] = "TAG_SECTION";
                newRollRow["ROLLMAP_CLCT_TYPE"] = cboBadtype.SelectedValue.ToString();

                if (sFlag == "N")
                {
                    //신규
                    newRollRow["CLCT_SEQNO"] = _collectSeq + 1;
                }
                else
                {
                    // 수정, 삭제
                    newRollRow["CLCT_SEQNO"] = _updatecollectSeq;
                }
                newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                newRollRow["ADJ_STRT_PSTN"] = txtStartPosition.Value;
                newRollRow["ADJ_END_PSTN"] = txtEndPosition.Value;

                if (sFlag == "N")
                {
                    newRollRow["STRT_PSTN"] = txtStartPosition.Value;
                    newRollRow["END_PSTN"] = txtEndPosition.Value;
                }
                else
                {
                    newRollRow["STRT_PSTN"] = Util.NVC_Decimal(popSTRT_PSTN);
                    newRollRow["END_PSTN"] = Util.NVC_Decimal(popEND_PSTN);
                }
                newRollRow["METHOD"] = sFlag;
                inRollmapTable.Rows.Add(newRollRow);


                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (sFlag == "D")
                        {  // 삭제
                            Util.MessageInfo("SFU1273"); // 삭제되었습니다.

                            cboBadSection.SelectedValue = "";
                            txtStartPosition.Value = 0;
                            txtEndPosition.Value = 0;
                            SetComboBox(cboBadSection);
                            IsUpdated = true;
                            DialogResult = MessageBoxResult.OK;
                        }
                        else
                        { // 저장
                            Util.MessageInfo("SFU1270"); // 저장되었습니다.

                            SetComboBadSection(cboBadSection);
                            cboBadSection.IsEnabled = true;
                            updateFlag = false;
                            IsUpdated = true;
                            DialogResult = MessageBoxResult.OK;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefectForRollMap_RP(string sFlag)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USER", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                //inTable.Columns.Add("FORCE_FLAG", typeof(string)); // Y 이면 다음 공정 투입 여부 체크 안함

                DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
                inRollmapTable.Columns.Add("LOTID", typeof(string));
                inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
                inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
                inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
                inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
                inRollmapTable.Columns.Add("ACTID", typeof(string));
                inRollmapTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
                inRollmapTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
                inRollmapTable.Columns.Add("ADJ_STRT_PSTN2", typeof(decimal));
                inRollmapTable.Columns.Add("ADJ_END_PSTN2", typeof(decimal));
                inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
                inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));
                inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
                inRollmapTable.Columns.Add("RESNCODE", typeof(string));   //활동유형코드

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = string.IsNullOrEmpty(_lotId) ? _runLotId : _lotId;
                newRow["WIPSEQ"] = string.IsNullOrEmpty(_wipseq) ? _runWipSeq : _wipseq;
                newRow["USER"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);


                DataRow newRollRow = inRollmapTable.NewRow();
                newRollRow["LOTID"] = string.IsNullOrEmpty(_lotId) ? _runLotId : _lotId;
                newRollRow["WIPSEQ"] = string.IsNullOrEmpty(_wipseq) ? _runWipSeq : _wipseq;
                newRollRow["EQPT_MEASR_PSTN_ID"] = "TAG_SECTION";
                newRollRow["ROLLMAP_CLCT_TYPE"] = cboBadtype.SelectedValue.ToString();
                newRollRow["RESNCODE"] = "";

                if (sFlag == "N")
                {
                    //신규
                    newRollRow["CLCT_SEQNO"] = _collectSeq + 1;
                }
                else
                {
                    // 수정, 삭제
                    newRollRow["CLCT_SEQNO"] = _updatecollectSeq;
                }
                newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";

                if (_Relative == "N" && sFlag == "N") //조성근 2025.04.09 ScrapSection 적용
                {
                    newRollRow["ADJ_STRT_PSTN2"] = txtStartPosition.Value;
                    newRollRow["ADJ_END_PSTN2"] = txtEndPosition.Value;
                }
                else
                {
                    newRollRow["ADJ_STRT_PSTN"] = txtStartPosition.Value;
                    newRollRow["ADJ_END_PSTN"] = txtEndPosition.Value;
                }

                if (sFlag == "N")
                {
                    newRollRow["STRT_PSTN"] = txtStartPosition.Value;
                    newRollRow["END_PSTN"] = txtEndPosition.Value;
                }
                else
                {
                    newRollRow["STRT_PSTN"] = Util.NVC_Decimal(popSTRT_PSTN);
                    newRollRow["END_PSTN"] = Util.NVC_Decimal(popEND_PSTN);
                }

                if (sFlag == "U" && Util.NVC_Decimal(popSTRT_PSTN) == Util.NVC_Decimal(popEND_PSTN)) sFlag = "D";    //시작 종료 좌표가 같으면 삭제

                newRollRow["METHOD"] = sFlag;
                inRollmapTable.Rows.Add(newRollRow);


                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (sFlag == "D")
                        {  // 삭제
                            Util.MessageInfo("SFU1273"); // 삭제되었습니다.
                            IsUpdated = true;
                            DialogResult = MessageBoxResult.OK;
                        }
                        else
                        { // 저장
                            Util.MessageInfo("SFU1270"); // 저장되었습니다.
                            IsUpdated = true;
                            DialogResult = MessageBoxResult.OK;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboBadSection_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

            try
            {
                updateFlag = false;

                DataTable dt = DataTableConverter.Convert(cboBadSection.ItemsSource);

                txtStartPosition.Value = 0;
                txtEndPosition.Value = 0;
                txtWidth.Value = 0;
                chkStartPosition.IsChecked = false;
                chkEndPosition.IsChecked = false;

                if (!CommonVerify.HasTableRow(dt) || cboBadSection.SelectedIndex < 0)
                {
                    DisplayControlByTagAutoFlag(string.Empty);
                    return;
                }

                DataRow dr = dt.Rows[cboBadSection.SelectedIndex];
                txtStartPosition.Value = dr["ADJ_STRT_PSTN"].GetDouble();
                txtEndPosition.Value = dr["ADJ_END_PSTN"].GetDouble();
                popSTRT_PSTN = $"{Util.NVC_Decimal(dr["ADJ_STRT_PSTN"].GetString()):0.0#}";
                popEND_PSTN = $"{Util.NVC_Decimal(dr["ADJ_END_PSTN"].GetString()):0.0#}";
                _updatecollectSeq = Util.NVC_Int(dr["CLCT_SEQNO"].GetString());
                txtWidth.Value = dr["ADJ_END_PSTN"].GetDouble() - dr["ADJ_STRT_PSTN"].GetDouble();

                if (CommonVerify.HasTableRow(dt))
                {
                    SelectProductQty();
                }

                DisplayControlByTagAutoFlag(dr["TAG_AUTO_FLAG"].GetString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkStartPosition_Checked(object sender, RoutedEventArgs e)
        {
            if (chkEndPosition.IsChecked == true)
            {
                chkEndPosition.IsChecked = false;
            }

            txtWidth.IsEnabled = true;
            txtStartPosition.IsEnabled = false;
            txtEndPosition.IsEnabled = true;
        }

        private void chkStartPosition_UnChecked(object sender, RoutedEventArgs e)
        {
            if (chkEndPosition.IsChecked == false)
            {
                txtStartPosition.IsEnabled = true;
                txtWidth.IsEnabled = false;
            }
        }

        private void chkEndPosition_Checked(object sender, RoutedEventArgs e)
        {
            if (chkStartPosition.IsChecked == true)
            {
                chkStartPosition.IsChecked = false;
            }
            txtWidth.IsEnabled = true;
            txtEndPosition.IsEnabled = false;
            txtStartPosition.IsEnabled = true;
        }

        private void chkEndPosition_UnChecked(object sender, RoutedEventArgs e)
        {
            if (chkStartPosition.IsChecked == false)
            {
                txtEndPosition.IsEnabled = true;
                txtWidth.IsEnabled = false;
            }
        }

        private void txtStartPosition_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (txtEndPosition.Value - txtStartPosition.Value < 0 && updateFlag == false)
                {
                    Util.MessageInfo("SFU8116", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtStartPosition.Focus();
                        }
                    }, ObjectDic.Instance.GetObjectName("시작위치"));
                }

                else
                {
                    txtWidth.Value = txtEndPosition.Value - txtStartPosition.Value;
                    SelectProductQty();
                }
            }
        }

        private void txtStartPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtEndPosition.Value - txtStartPosition.Value < 0 && updateFlag == false)
            {
                Util.MessageInfo("SFU8116", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtEndPosition.Focus();
                    }
                }, ObjectDic.Instance.GetObjectName("종료위치"));
            }
            else
            {
                txtWidth.Value = txtEndPosition.Value - txtStartPosition.Value;
                SelectProductQty();
            }
        }

        private void txtEndPosition_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (txtEndPosition.Value - txtStartPosition.Value < 0)
                {
                    Util.MessageInfo("SFU8116", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEndPosition.Focus();
                        }
                    }, ObjectDic.Instance.GetObjectName("종료위치"));

                }
                else
                {
                    txtWidth.Value = txtEndPosition.Value - txtStartPosition.Value;
                    SelectProductQty();
                }
            }
        }

        private void txtEndPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (chkStartPosition.IsChecked == true)
            //{
            //    txtWidth.Value = txtEndPosition.Value - txtStartPosition.Value;
            //}
        }

        private void txtWidth_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (chkStartPosition.IsChecked == false && chkEndPosition.IsChecked == false)
                {
                }
                else
                {
                    if (chkStartPosition.IsChecked == true)
                    {
                        txtEndPosition.Value = txtStartPosition.Value + txtWidth.Value;
                    }
                    else if (chkEndPosition.IsChecked == true)
                    {
                        txtStartPosition.Value = txtEndPosition.Value - txtWidth.Value;
                    }
                    SelectProductQty();
                }
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            txtStartPosition.Value = 0;
            txtEndPosition.Value = 0;
            txtWidth.Value = 0;

            txtTopQty.Value = double.NaN;
            txtBackQty.Value = double.NaN;
            txtFoilQty.Value = double.NaN;

            InitClear();
        }
        #endregion

        #region Mehod

        void SetComboBox(C1ComboBox cbo)
        {
            switch (cbo.Name)
            {
                case "cboBadSection":
                    SetComboBadSection(cbo);
                    break;
                case "cboBadtype":
                    CommonCombo.CommonBaseCombo("DA_PRD_SEL_RM_RPT_BADSECTION", cbo, new string[] { "LANGID", "PROCID", "EQPTID" }, new string[] { LoginInfo.LANGID, _processCode, _equipmentCode }, CommonCombo.ComboStatus.NONE, "CBO_CODE", "CBO_NAME", _collectType);
                    break;
            }
        }

        private void CheckReadonly()
        {
            DataTable newDt = new DataTable();
            newDt.TableName = "RQSTDT";
            newDt.Columns.Add("ADJ_LOTID", typeof(string));
            newDt.Columns.Add("CLCT_SEQNO", typeof(int));
            newDt.Columns.Add("ADJ_WIPSEQ", typeof(int));

            DataRow drTag = newDt.NewRow();
            drTag["ADJ_LOTID"] = _runLotId;
            drTag["CLCT_SEQNO"] = _collectSeq;
            drTag["ADJ_WIPSEQ"] = _runWipSeq;
            newDt.Rows.Add(drTag);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_CHECKFLAG", "RQSTDT", "RSLTDT", newDt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["TAG_AUTO_FLAG"].ToString() == "Y")
                {
                    chkStartPosition.IsChecked = false;
                    chkStartPosition.IsEnabled = false;
                    txtStartPosition.IsEnabled = false;
                    txtWidth.IsEnabled = false;
                    chkEndPosition.IsChecked = false;
                    chkEndPosition.IsEnabled = false;
                    txtEndPosition.IsEnabled = false;

                    updateFlag = false;
                }
            }
        }

        private void InitClear()
        {
            cboBadSection.Clear();

            chkStartPosition.IsChecked = false;
            chkStartPosition.IsEnabled = true;
            txtStartPosition.IsEnabled = true;
            txtWidth.IsEnabled = true;
            chkEndPosition.IsChecked = false;
            chkEndPosition.IsEnabled = true;
            txtEndPosition.IsEnabled = true;

            popSTRT_PSTN = string.Empty;
            popEND_PSTN = string.Empty;
            cboBadSection.Text = string.Empty;

            // 콤보박스 리드온리 처리
            cboBadSection.IsEnabled = false;
            SetComboBox(cboBadtype);
            updateFlag = true;

            _lotId = string.Empty;
            _wipseq = string.Empty;
        }

        private void SetComboBadSection(C1ComboBox cbo)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_RM_RPT_BAD";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("ADJ_LOTID", typeof(string));
                inTable.Columns.Add("ADJ_WIPSEQ", typeof(decimal));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["ADJ_LOTID"] = _runLotId;
                dr["ADJ_WIPSEQ"] = _runWipSeq;
                dr["PROCID"] = _processCode;
                dr["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (Util.NVC_Int(dtResult.Rows[i]["CLCT_SEQNO"].ToString()) == Util.NVC_Int(_collectSeq))
                    {
                        cbo.SelectedIndex = i;
                        _lotId = dtResult.Rows[i]["LOTID"].GetString();
                        _wipseq = dtResult.Rows[i]["WIPSEQ"].GetString();
                        DisplayControlByTagAutoFlag(dtResult.Rows[i]["TAG_AUTO_FLAG"].GetString());
                        return;
                    }
                }

                cbo.SelectedIndex = 0;
                DisplayControlByTagAutoFlag(CommonVerify.HasTableRow(dtResult) ? dtResult.Rows[0]["TAG_AUTO_FLAG"].GetString() : string.Empty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DisplayControlByTagAutoFlag(string tagAutoFlag)
        {
            if (tagAutoFlag == "Y")
            {
                chkStartPosition.IsChecked = false;
                chkStartPosition.IsEnabled = false;
                txtStartPosition.IsEnabled = false;

                txtWidth.IsEnabled = false;

                chkEndPosition.IsChecked = false;
                chkEndPosition.IsEnabled = false;
                txtEndPosition.IsEnabled = false;
            }
            else
            {
                chkStartPosition.IsEnabled = true;
                txtStartPosition.IsEnabled = true;
                txtWidth.IsEnabled = true;
                chkEndPosition.IsEnabled = true;
                txtEndPosition.IsEnabled = true;
                chkStartPosition.IsChecked = true;

                if (_isLoaded == false) txtStartPosition.IsEnabled = false;
            }
        }

        private void SelectProductQty()
        {
            //Top, Back 양품량, Foil 양
            const string bizRuleName = "DA_PRD_SEL_RM_RPT_PROD_QTY";
            DataTable inTable = new DataTable();
            inTable.TableName = "RQSTDT";
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(int));
            inTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
            inTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = _runLotId;
            newRow["WIPSEQ"] = _runWipSeq;
            newRow["ADJ_STRT_PSTN"] = txtStartPosition.Value;
            newRow["ADJ_END_PSTN"] = txtEndPosition.Value;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                txtTopQty.Value = dtResult.Rows[0]["TOP_PROD_QTY"].GetDouble();
                txtBackQty.Value = dtResult.Rows[0]["BACK_PROD_QTY"].GetDouble();
                txtFoilQty.Value = dtResult.Rows[0]["FOIL_QTY"].GetDouble();
            }
            else
            {
                txtTopQty.Value = double.NaN;
                txtBackQty.Value = double.NaN;
                txtFoilQty.Value = double.NaN;
            }
        }

        #endregion


    }
}
