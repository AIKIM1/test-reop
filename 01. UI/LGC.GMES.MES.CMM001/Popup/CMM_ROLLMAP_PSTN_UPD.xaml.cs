/*************************************************************************************
 Created Date : 2021.03.05
      Creator : 오광택
   Decription : ROLLMAP PSTN 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.05  : Initial Created.
  2021.03.29  : 신광희 시작위치, 종료위치 체크박스 길이 추가 및 컨트롤(C1NumericBox) 변경
  2023.01.10  : 윤기업 ROLLMAP 수불 반영 - TAG_SECTION 추가/수정 후 불량집계 재산출
  2023.01.13  : 윤기업 ROLLMAP 수불 반영 - TAG_SECTION 신규/수정/삭제 BIZ 하나로 통합
  2023.02.02  : 신광희 BACK(양품량), TOP(양품량), FOIL 컨트롤 추가 (FOIL 값은 시작위치와 종료위치의 차이(길이)로 표시 > 추후 변경될 수 있음) 
  2023.07.04  : 신광희 RollMap 구간불량 수정 화면은 소형2170과 동일한 화면으로 같이 사용(_isRollMapPatternApply 파라메터 값으로 BizRule 적용 처리)
  2024.06.20  : 박학철 START, END 좌표 가져올때 소수점 1자리 만 가져와 수동 구간불량 등록 할 수 있도록 수정
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

    public partial class CMM_ROLLMAP_PSTN_UPD : C1Window, IWorkArea
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
        private bool _isRollMapPatternApply = false;


        public bool IsUpdated { get; set; }

        public CMM_ROLLMAP_PSTN_UPD()
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
            txtStartPosition.Value = Math.Round(popSTRT_PSTN.GetDouble(),1);
            txtEndPosition.Value = Math.Round(popEND_PSTN.GetDouble(),1);
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

                _updatecollectSeq = _collectSeq;
                popSTRT_PSTN = _startPosition;
                popEND_PSTN = _endPosition;

                btnSave.Visibility = _saveButtonVisibility;
                btnDel.Visibility = _saveButtonVisibility;

                if (parameters.Length > 11)
                {
                    _isRollMapPatternApply = (bool)parameters[11];
                    Header = ObjectDic.Instance.GetObjectName("ROLLMAP_TAG_SECTION_SAVE_PATTERN");
                }
                else
                    Header = ObjectDic.Instance.GetObjectName("ROLLMAP_TAG_SECTION_SAVE");
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
                    txtStartPosition.Value = Math.Round(popSTRT_PSTN.GetDouble(),1);
                    txtEndPosition.Value = Math.Round(popEND_PSTN.GetDouble(),1);
                    txtWidth.Value = Math.Round(popEND_PSTN.GetDouble(),1) - Math.Round(popSTRT_PSTN.GetDouble(),1);
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
                SaveDefectForRollMap("D");
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
                string bizRuleName = (_isRollMapPatternApply) ? "BR_PRD_REG_TAG_SECTION_CY" : "BR_PRD_REG_TAG_SECTION"; 
                
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

                if (_isRollMapPatternApply)
                {
                    inRollmapTable.Columns.Add("ADJ_STRT_PTN_PSTN", typeof(decimal));
                    inRollmapTable.Columns.Add("ADJ_END_PTN_PSTN", typeof(decimal));
                    inRollmapTable.Columns.Add("STRT_PTN_PSTN", typeof(decimal));
                    inRollmapTable.Columns.Add("END_PTN_PSTN", typeof(decimal));
                }
                else
                {
                    inRollmapTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
                    inRollmapTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
                    inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
                    inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));
                }
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

                if(_isRollMapPatternApply)
                {
                    newRollRow["ADJ_STRT_PTN_PSTN"] = txtStartPosition.Value;
                    newRollRow["ADJ_END_PTN_PSTN"] = txtEndPosition.Value;

                    if (sFlag == "N")
                    {
                        newRollRow["STRT_PTN_PSTN"] = txtStartPosition.Value;
                        newRollRow["END_PTN_PSTN"] = txtEndPosition.Value;
                    }
                    else
                    {
                        newRollRow["STRT_PTN_PSTN"] = Util.NVC_Decimal(popSTRT_PSTN);
                        newRollRow["END_PTN_PSTN"] = Util.NVC_Decimal(popEND_PSTN);
                    }
                }
                else
                {
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
                }

                newRollRow["METHOD"] = sFlag;
                inRollmapTable.Rows.Add(newRollRow);

                //string xml = inDataSet.GetXml();

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

                if(_isRollMapPatternApply)
                {
                    txtStartPosition.Value = dr["ADJ_STRT_PTN_PSTN"].GetDouble();
                    txtEndPosition.Value = dr["ADJ_END_PTN_PSTN"].GetDouble();
                    popSTRT_PSTN = $"{Util.NVC_Decimal(dr["ADJ_STRT_PTN_PSTN"].GetString()):0.0#}";
                    popEND_PSTN = $"{Util.NVC_Decimal(dr["ADJ_END_PTN_PSTN"].GetString()):0.0#}";
                    _updatecollectSeq = Util.NVC_Int(dr["CLCT_SEQNO"].GetString());
                    txtWidth.Value = dr["ADJ_END_PTN_PSTN"].GetDouble() - dr["ADJ_STRT_PTN_PSTN"].GetDouble();
                }
                else
                {
                    txtStartPosition.Value = dr["ADJ_STRT_PSTN"].GetDouble();
                    txtEndPosition.Value = dr["ADJ_END_PSTN"].GetDouble();
                    popSTRT_PSTN = $"{Util.NVC_Decimal(dr["ADJ_STRT_PSTN"].GetString()):0.0#}";
                    popEND_PSTN = $"{Util.NVC_Decimal(dr["ADJ_END_PSTN"].GetString()):0.0#}";
                    _updatecollectSeq = Util.NVC_Int(dr["CLCT_SEQNO"].GetString());
                    txtWidth.Value = dr["ADJ_END_PSTN"].GetDouble() - dr["ADJ_STRT_PSTN"].GetDouble();
                }

                if(CommonVerify.HasTableRow(dt))
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
                    CommonCombo.CommonBaseCombo("DA_PRD_SEL_ROLLMAP_BADSECTION", cbo, new string[] { "LANGID", "PROCID", "EQPTID" }, new string[] { LoginInfo.LANGID, _processCode, _equipmentCode }, CommonCombo.ComboStatus.NONE, "CBO_CODE", "CBO_NAME", _collectType);
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

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLMAP_CHECKFLAG", "RQSTDT", "RSLTDT", newDt);

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
                string bizRuleName = (_isRollMapPatternApply) ? "DA_PRD_SEL_ROLLMAP_BAD_CY" : "DA_PRD_SEL_ROLLMAP_BAD";

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
            string bizRuleName = (_isRollMapPatternApply) ? "DA_PRD_SEL_ROLLMAP_PROD_QTY_CY" : "DA_PRD_SEL_ROLLMAP_PROD_QTY";
            DataTable inTable = new DataTable();
            inTable.TableName = "RQSTDT";
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("CLCT_SEQNO", typeof(int));

            if(_isRollMapPatternApply)
            {
                inTable.Columns.Add("ADJ_STRT_PTN_PSTN", typeof(decimal));
                inTable.Columns.Add("ADJ_END_PTN_PSTN", typeof(decimal));
            }
            else
            {
                inTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
                inTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            }

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = _runLotId;
            newRow["WIPSEQ"] = _runWipSeq;

            if (_isRollMapPatternApply)
            {
                newRow["ADJ_STRT_PTN_PSTN"] = txtStartPosition.Value;
                newRow["ADJ_END_PTN_PSTN"] = txtEndPosition.Value;
            }
            else
            {
                newRow["ADJ_STRT_PSTN"] = txtStartPosition.Value;
                newRow["ADJ_END_PSTN"] = txtEndPosition.Value;
            }
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
