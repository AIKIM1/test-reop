/*************************************************************************************
 Created Date : 2016.08.19
      Creator : 
   Decription : 작업시작 대기 Lot List 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  2017.01.12  : CMM으로 SLURRY 팝업 이동
  2022.02.10  : 롤맵 분기 적용 (이송보고 및 종료되지 않은 SLURRY를 Default로 사용)
  2024.01.19  : [E20240115-000246] 이송보고 연동 체크 박스 활성/비활성 기능 추가
  2024.08.10  : 배현우 [E20240807-000861] : Coater 공정진척 Dam Slurry 투입위치 추가 및 설비 및 슬러리 조회 비즈 분기 처리
  2025.04.30  : 조범모 [MI_LS_OSS_0117] : 슬러리 장착시 더블레이어코팅설비일 경우 코팅면의 위치(Upper/Lower)로 대상호기 기준정보로 선별
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_SLURRY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SLURRY : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _LOTID = string.Empty;
        private string _PRODID = string.Empty;
        private string _PRDT_CLSS_CODE = string.Empty;
        private string _EQSGID = string.Empty;
        private string _WOID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _SLURRYID = string.Empty;
        private string _LotType = string.Empty;
        private string _LotTypeName = string.Empty;
        private string _SLotType = string.Empty;
        private string _SLotTypeName = string.Empty;
        private string _SURFACE = string.Empty;

        private int _POSITION = -1;

        private bool isSingleCoater = false;
        private bool isAllConfirm = false;
        private bool isSlurryTerm = false;

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }
        public string _ReturnCLSSCODE
        {
            get { return _PRDT_CLSS_CODE; }
        }
        public string _ReturnPRODID
        {
            get { return _PRODID; }
        }
        public int _ReturnPosition
        {
            get { return _POSITION; }
        }
        public bool _IsAllConfirm
        {
            get { return isAllConfirm; }
        }

        public bool _IsSlurryTerm
        {
            get { return isSlurryTerm; }
        }

        public string _ReturnSLotType
        {
            get { return _SLotType; }
        }

        public string _sReturnSLotTypeName
        {
            get { return _SLotTypeName; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_ELEC_SLURRY()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }

            _PROCID = Util.NVC(tmps[0]);
            _EQSGID = Util.NVC(tmps[1]);
            _WOID = Util.NVC(tmps[2]);

            // EQPTID 추가
            _EQPTID = Util.NVC(tmps[3]);

            // 이전 POSITION정보 기억용
            _POSITION = Util.NVC_Int(tmps[4]);

            // SLURRY 코터 적용 방식 변경으로 인하여 추가 (2017-02-11)
            _SLURRYID = Util.NVC(tmps[5]);
            isSingleCoater = string.Equals(Util.NVC(tmps[6]), "Y") ? true : false;  // 단면코터 확인

            if (isSingleCoater)
                chkCoater.Visibility = Visibility.Collapsed;

            // 소형3동은 체크안하도록 요청으로 인하여 특화 로직 추가 => 광폭여부로 체크하는걸로 변경 [2017-07-03]
            //if (string.Equals(LoginInfo.CFG_AREA_ID, "E4"))
            if (string.Equals(Util.NVC(tmps[7]), "Y"))
                chkCoater.IsChecked = false;

            // [MI_LS_OSS_0117] : 슬러리 장착시 더블레이어코팅설비일 경우 코팅면의 위치(Upper/Lower)로 대상호기 기준정보로 선별
            _SURFACE = Util.NVC(tmps[8]);

            //CSR C20210518-000221 코팅 공정진척 시, 투입 Slurry Lot 유형에 대한 Validation
            //_LotType = Util.NVC(tmps[8]);
            //_LotTypeName = Util.NVC(tmps[9]);

            SetMixerDefaultCombo(); // MIXER DEFAULT 추가

            // 롤맵 여부 추가
            if (IsEquipmentAttr() == true && IsRollMapBatchLink() == true)
                chkRollMapBatchLink.Visibility = Visibility.Visible;

            // 배치 연계 기능 활성화 (E20240115-000246) 2024.01.19 JEONG KI TONG
            chkRollMapBatchLink.IsEnabled = false;
            if (IsEquipmentAttr() == true && chkRollMapBatchLink.Visibility == Visibility.Visible && IsRollMapBatchLinkEnable() == true)
                chkRollMapBatchLink.IsEnabled = true;

            if (_POSITION == 4)
            {
                chkCoater.Visibility = Visibility.Collapsed;
                chkCoater.IsChecked = false;
            }

            GetLotList(Convert.ToString(cboEquipment.SelectedValue));
        }
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1651");    //선택된 항목이 없습니다.
                return;
            }

            if (string.Equals(_SLURRYID, _LOTID))
            {
                Util.MessageValidation("SFU3591");  //이미 장착되어 있는 Batch 입니다.
                return;
            }

            //CSR C20210518-000221 코팅 공정진척 시, 투입 Slurry Lot 유형에 대한 Validation
            /*if (_LotType != string.Empty && _LotType != _SLotType)
            {
                Util.MessageConfirm("SFU8399", (Result) =>
                {
                    if (Result != MessageBoxResult.OK)
                        return;

                    //이전 Slurry 종료 체크시 확인 안함.
                    if (chkSlurry.Visibility == Visibility.Visible && chkSlurry.IsChecked == false)
                    {
                        SetPreSelectedMixerInfo();

                        if (chkCoater.Visibility == Visibility.Visible && chkCoater.IsChecked == true)
                            isAllConfirm = true;

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    else
                    {
                        //사용중인 Batch[{%1}]를 종료시키고, 선택한 Batch[{%2}]로 장착 처리하시겠습니까?
                        Util.MessageConfirm("SFU3223", (vResult) =>
                        {
                            if (vResult == MessageBoxResult.OK)
                            {
                                // Mixer설비 선 저장
                                SetPreSelectedMixerInfo();

                                if (chkCoater.Visibility == Visibility.Visible && chkCoater.IsChecked == true)
                                    isAllConfirm = true;

                                // Slurry Term처리
                                isSlurryTerm = true;

                                this.DialogResult = MessageBoxResult.OK;
                            }
                        }, new object[] { _SLURRYID, _LOTID });
                    }

                }, new object[] { _LotTypeName, _SLotTypeName });
            }
            else
            */
            {
                //이전 Slurry 종료 체크시 확인 안함.
                if (chkSlurry.Visibility == Visibility.Visible && chkSlurry.IsChecked == false)
                {
                    SetPreSelectedMixerInfo();

                    if (chkCoater.Visibility == Visibility.Visible && chkCoater.IsChecked == true)
                        isAllConfirm = true;

                    this.DialogResult = MessageBoxResult.OK;
                }
                else
                {
                    //사용중인 Batch[{%1}]를 종료시키고, 선택한 Batch[{%2}]로 장착 처리하시겠습니까?
                    Util.MessageConfirm("SFU3223", (vResult) =>
                    {
                        if (vResult == MessageBoxResult.OK)
                        {
                            // Mixer설비 선 저장
                            SetPreSelectedMixerInfo();

                            if (chkCoater.Visibility == Visibility.Visible && chkCoater.IsChecked == true)
                                isAllConfirm = true;

                            // Slurry Term처리
                            isSlurryTerm = true;

                            this.DialogResult = MessageBoxResult.OK;
                        }
                    }, new object[] { _SLURRYID, _LOTID });
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _LOTID = string.Empty;
            _PRODID = string.Empty;
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                //DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                dgLotInfo.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID").ToString();
                _PRODID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();
                _PRDT_CLSS_CODE = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRDT_CLSS_CODE").ToString();
                //CSR C20210518-000221 코팅 공정진척 시, 투입 Slurry Lot 유형에 대한 Validation
                //기존 LOTTYPE 명칭을 LOTTYPE로 가져오므로 LOTTYPE_CODE 추가
                //_SLotTypeName = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTTYPE").ToString();
                //_SLotType = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTTYPE_CODE").ToString();

                // REMARK 추가
                // 2024.10.14. 김영국 - Remark 값이 null인 경우가 발생하여 삼항식으로 변경함.
                //txtRemark.Text = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "REMARK").ToString();
                txtRemark.Text = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "REMARK") == null ? "" : DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "REMARK").ToString();
            }
        }

        private void cboEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.OldValue < 0)
                return;

            Util.gridClear(dgLotInfo);
            GetLotList(Convert.ToString(((System.Data.DataRowView)(cboEquipment.Items[e.NewValue])).Row.ItemArray[0]));
            //GetLotList(Convert.ToString(((System.Data.DataRowView)(cboEquipment.Items[e.NewValue])).Row.ItemArray[2])); // 2024.10.14. 김영국 - Item Array의 순서가 변경 됨에 따라 ITEM값 가져오는 Index 변경.
        }
        #endregion

        #region Mehod
        private void SetMixerDefaultCombo()
        {
            try
            {
                string bizRuleID = _POSITION != 4 ? "BR_PRD_SEL_EQUIPMENT_PRE_MIXER_INFO" : "DA_PRD_SEL_EQUIPMENT_DAM_MIXER_INFO"; // 2024-08-10 배현우  Dam Mixer 설비 조회 쿼리 분기


                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SURFACE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = _EQSGID;
                Indata["EQPTID"] = _EQPTID;
                if (string.IsNullOrEmpty(_SURFACE) == false) Indata["SURFACE"] = _SURFACE;

                IndataTable.Rows.Add(Indata);

                DataTable dtMixer = new ClientProxy().ExecuteServiceSync(bizRuleID, "INDATA", "RSLTDT", IndataTable);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";
                cboEquipment.ItemsSource = DataTableConverter.Convert(dtMixer);

                int iSelectedIdx = 0;
                for (int i = 0; i < dtMixer.Rows.Count; i++)
                {
                    if (string.Equals(Convert.ToString(dtMixer.Rows[i]["USEFLAG"]), "Y"))
                    {
                        iSelectedIdx = i;
                        break;
                    }
                }
                cboEquipment.SelectedIndex = iSelectedIdx;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetLotList(string sMixerId)
        {
            try
            {

                string bizRuleID = _POSITION != 4 ? "DA_PRD_SEL_SLURRY_WIP" : "DA_PRD_SEL_DAM_SLURRY_WIP";    // 2024-08-10 배현우  Dam Slurry 조회 쿼리 분기

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                IndataTable.Columns.Add("COT_EQPTID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("INPUT_LOTID", typeof(string));
                IndataTable.Columns.Add("MOVE_FLAG", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = _EQSGID;
                Indata["PROCID"] = _PROCID;
                Indata["WO_DETL_ID"] = _WOID;
                Indata["COT_EQPTID"] = _EQPTID;

                // EQPTID 추가
                if (!string.IsNullOrEmpty(sMixerId))
                    Indata["EQPTID"] = sMixerId;

                // 장착 SLURRY 추가
                if (!string.IsNullOrEmpty(_SLURRYID))
                    Indata["INPUT_LOTID"] = _SLURRYID;

                // 이송FLAG 추가
                if (chkRollMapBatchLink.IsVisible == true && chkRollMapBatchLink.IsChecked == true)
                    Indata["MOVE_FLAG"] = "Y";

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(bizRuleID, "INDATA", "RSLTDT", IndataTable);
                //if (dtMain.Rows.Count > 0)
                //{
                //dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
                Util.GridSetData(dgLotInfo, dtMain, FrameOperation, true);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPreSelectedMixerInfo()
        {
            // Selected ALL
            if (string.IsNullOrEmpty(Convert.ToString(cboEquipment.SelectedValue)))
                return;

            // Selected Equals
            if (string.Equals(Convert.ToString(((System.Data.DataRowView)(cboEquipment.Items[cboEquipment.SelectedIndex])).Row.ItemArray[2]), "Y"))
                return;

            // SET
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("PRE_BAS_EQPTID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));   // 2024.10.14. 김영국 - 호출되는 BR 변경.

            DataRow Indata = IndataTable.NewRow();
            Indata["EQPTID"] = _EQPTID;
            Indata["PRE_BAS_EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
            Indata["USERID"] = LoginInfo.USERID;   // 2024.10.14. 김영국 - 호출되는 BR 변경.

            IndataTable.Rows.Add(Indata);

            //new ClientProxy().ExecuteService("DA_BAS_UPD_EIOATTR_PRE_BAS_EQPTID", "INDATA", null, IndataTable, (result, ex) =>
            new ClientProxy().ExecuteService("BR_PRD_REG_EIOATTR_PRE_BAS_EQPTID", "INDATA", null, IndataTable, (result, ex) => // 2024.10.14. 김영국 - 호출되는 BR 변경.
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
            });
        }
        #endregion

        #region 롤맵 기능 적용
        private bool IsEquipmentAttr()
        {
            try
            {
                DataRow[] dr = Util.getEquipmentAttr(_EQPTID).Select();
                if (dr?.Length > 0)
                {
                    if (string.Equals(Util.NVC(dr[0]["ROLLMAP_EQPT_FLAG"]), "Y"))
                        return true;
                }
            }
            catch (Exception ex) { }

            return false;
        }

        private bool IsRollMapBatchLink()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                dt.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ROLLMAP_MIX_COT_LINK_EQPT";
                dr["COM_CODE"] = _EQPTID;

                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);

                if (result.Rows.Count > 0 && string.Equals(result.Rows[0]["ATTR5"], "Y"))
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        /// <summary>
        /// 이송 보고 연동 체크 박스 활성화 여부
        /// </summary>
        /// <returns></returns>
        private bool IsRollMapBatchLinkEnable()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                dt.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ROLLMAP_CONSTANTS";
                dr["COM_CODE"] = "IS_ENABLE_BATCH_LINK_CHKBOX_FOR_CMM_ELEC_SLURRY_UI";

                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);

                if (result.Rows.Count > 0 && string.Equals(result.Rows[0]["ATTR1"], "Y"))
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void chkRollMapBatchLink_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox != null && cboEquipment != null)
                GetLotList(Convert.ToString(cboEquipment.SelectedValue));
        }

        #endregion


    }
}
