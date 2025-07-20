/*************************************************************************************
 Created Date : 2016.07.19
      Creator : 오화백
   Decription : 재작업 대상 이동
--------------------------------------------------------------------------------------
 [Change History]
  2016.07.19  오화백 : Initial Created.
  2019.12.09  이현호 : CSR ID : 4077158 활성화 재고이동 화면에서 이동구분이 ASSY_REWORK(조립 재작업)이며 이동공정이 A4100(X-Ray 재작업)일 경우 실적차감 여부 모두 해제
  2023.03.16  이병윤 : CSR ID : C20221109-000484 출고 완료된 Pallet에 대해 출고 취소하지 않아도 출고 취소하겠냐는 알림창이 뜨면서 출고 취소가 가능하도록 변경
  2023.03.22  이병윤 : CSR ID : C20221109-000484 DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST -> DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST_MOVING 변경
  2023.03.31  성민식 : CSR ID : E20230120-000167 탭별 조회항목 추가(DEFECT_NAME), 재공이동 취소 탭 조회 분기 오류 수정
  2023.09.12  이병윤 : CSR ID : E20230727-001286 활성화 재공 이동 이력,활성화 재공 이동 취소 palletId 다중 입력 가능 추가


**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_093 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        string _ACTDTTM = string.Empty;
        string _CALDATE = string.Empty;
        string _LOTID = string.Empty;
        string _PRODID = string.Empty;
        string _EQSGID = string.Empty;
        string _MOVE_TYPE = string.Empty;
        string _FROM_PROCESS = string.Empty;
        string _TO_PROCESS = string.Empty;
        string _ROUTID = string.Empty;
        string _FLOWID = string.Empty;
       
        Decimal _WIPQTY2_ED;
        DataTable _MinusProcess = null;
        int RowCount = 0;


        public COM001_093()
        {
            InitializeComponent();
            InitCombo();

            this.Loaded += UserControl_Loaded;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            #region [활성화 재공 이동 등록]

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegmentInput };
            _combo.SetCombo(cboAreaInput, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaInput };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcessInput };
            _combo.SetCombo(cboEquipmentSegmentInput, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegmentInput };
            String[] sFilterProcess = { "SEARCH", "Y", string.Empty, string.Empty };
            _combo.SetCombo(cboProcessInput, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcess, sCase: "PROCESS_SORT", cbParent: cbProcessParent);

            String[] sFilterQmsRequest = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQlty, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequest, sCase: "COMMCODES");

            // 대상동
            //C1ComboBox[] cboTagetAreaChild = { cboTagetLine };
            _combo.SetCombo(cboTagetArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            //대상라인
            _combo.SetCombo(cboTagetLine, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT");

            //대상 공정
            _combo.SetCombo(cboReworkProcess, CommonCombo.ComboStatus.SELECT, sCase: "PROCESS");

            //이동구분
            String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE", "", "" };
            _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");



            //if (cboEquipmentSegmentInput.SelectedValue != null)
            //{
            //    CommonCombo _combo = new CommonCombo();
            //    if (cboEquipmentSegmentInput.SelectedValue.ToString() == "M1CF2") // 초소형
            //    {
            //        // 초소형 이동구분
            //        String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE", "CS", String.Empty };
            //        _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");
            //    }
            //    else
            //    {
            //        // 원각 이동구분
            //        String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE", "", "" };
            //        _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");
            //    }
            //}

            cboTagetArea.SelectedIndex = 0;
            cboTagetLine.SelectedIndex = 0;
            cboReworkProcess.SelectedIndex = 0;
            cboRework.SelectedIndex = 0;

            //C20181121_50523 특성/Grader 공정으로 인한 메시지 출력 처리 
            if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
            {
               txtMessage.Text =  ObjectDic.Instance.GetObjectName("F5300_차감선택_MSG");
            }

            #endregion

            #region [활성화 재공 이동 이력, 이력 취소]
            //동
            C1ComboBox[] cboAreaHistoryChild = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaHistoryChild);
            
            C1ComboBox[] cboAreaHistoryChildCancel = { cboEquipmentSegmentHistoryCancel };
            _combo.SetCombo(cboAreaHistoryCancel, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaHistoryChildCancel);


            //라인
            C1ComboBox[] cboEquipmentSegmentHistoryParent = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentHistoryChild = { cboProcessHistory, cboProcessMove };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentHistoryChild, cbParent: cboEquipmentSegmentHistoryParent);

            C1ComboBox[] cboEquipmentSegmentHistoryParentCancel = { cboAreaHistoryCancel };
            C1ComboBox[] cboEquipmentSegmentHistoryChildCancel = { cboProcessHistoryCancel, cboProcessMoveCancel };
            _combo.SetCombo(cboEquipmentSegmentHistoryCancel, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentHistoryChildCancel, cbParent: cboEquipmentSegmentHistoryParentCancel);


            //공정
            C1ComboBox[] cbProcessHistoryParent = { cboEquipmentSegmentHistory };
            String[] sFilterProcessHistory = { "SEARCH", "Y", string.Empty, string.Empty };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilterProcessHistory, sCase: "PROCESS_SORT", cbParent: cbProcessHistoryParent);

            C1ComboBox[] cbProcessHistoryParentCancel = { cboEquipmentSegmentHistoryCancel };
            String[] sFilterProcessHistoryCancel = { "SEARCH", "Y", string.Empty, string.Empty };
            _combo.SetCombo(cboProcessHistoryCancel, CommonCombo.ComboStatus.ALL, sFilter: sFilterProcessHistoryCancel, sCase: "PROCESS_SORT", cbParent: cbProcessHistoryParentCancel);


            //구분
            String[] sFilterElectypeHistory = { "", "FORM_LOT_MOVE_TYPE", "", "" };
            _combo.SetCombo(cboReworkHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectypeHistory, sCase: "FORM_MOVE_COMMCODES");

            String[] sFilterElectypeHistoryCancel = { "", "FORM_LOT_MOVE_TYPE", "", "" };
            _combo.SetCombo(cboReworkHistoryCancel, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectypeHistoryCancel, sCase: "FORM_MOVE_COMMCODES");


            C1ComboBox[] cbProcessMoveParent = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboProcessMove, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_MOVE", cbParent: cbProcessMoveParent);

            C1ComboBox[] cbProcessMoveParentCancel = { cboEquipmentSegmentHistoryCancel };
            _combo.SetCombo(cboProcessMoveCancel, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_MOVE", cbParent: cbProcessMoveParentCancel);

            #endregion


        }
        #endregion

        #region Event
        #region [활성화 재공 이동 등록]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                DataTable dtInfo = ary.GetValue(0) as DataTable;

                foreach (DataRow dr in dtInfo.Rows)
                {
                    txtPalletIdInput.Text = Util.NVC(dr["PALLETID"]);
                    GetLotList(false);
                }
            }

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReworkMove);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }
        private void btnSearchInput_Click(object sender, RoutedEventArgs e)
        {
            GetLotList(true);
        }

        /// <summary>
        /// C20181003_08081 다중 입력 가능 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPalletIdInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();


                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                            break;

                        if (dgListInput.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgListInput.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgListInput.Rows[idx].DataItem, "PALLETID").ToString() == sPasteStrings[i])
                                {
                                    dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                    dgListInput.SelectedIndex = i;
                                    txtPalletIdInput.Focus();
                                    txtPalletIdInput.Text = string.Empty;
                                    return;
                                }
                            }
                        }

                        GetLotList(false, sPasteStrings[i]);
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

        //Pallet ID 클릭시
        private void txtPalletIdInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletIdInput.Text.Trim();
                    if (dgListInput.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                dgListInput.SelectedIndex = i;
                                txtPalletIdInput.Focus();
                                txtPalletIdInput.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        //이동코드 클릭시 대상동,대상라인,대상공정,차감할 공정을 셋팅한다.
        private void cboRework_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboRework.SelectedValue != null)
            {
                if (cboRework.SelectedIndex != 0)
                {
                    if (dgSelectInput.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU4163"); //대상목록정보가 없습니다.
                        cboRework.SelectedIndex = 0;
                        return;

                    }
                    if (cboRework.SelectedValue.ToString() == "MOVE_PROC")
                    {
                        //대상동을 선택 
                        cboTagetArea.IsEnabled = false;
                        cboTagetArea.SelectedValue = DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "AREAID").ToString();
                        //대상라인
                        cboTagetLine.SelectedIndex = 0;
                        TagetLine("MOVE_PROC");
                        cboTagetLine.IsEnabled = true;

                        //이동공정
                        CommonCombo _combo = new CommonCombo();
                        String[] sFilterElectype = { cboTagetLine.SelectedValue.ToString(), DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "ROUTID").ToString(), DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "FLOWID").ToString(), DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PROCID").ToString() };
                        _combo.SetCombo(cboReworkProcess, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "TARGET_PROCESS");
                        cboReworkProcess.IsEnabled = true;
                        //초기화
                        if (dgSelectInput_Prcess.Rows.Count > 0)
                        {
                            Util.gridClear(dgSelectInput_Prcess);
                        }
                        if (cboReworkProcess.Items.Count == 2)
                        {
                            cboReworkProcess.SelectedIndex = 1;
                            if (cboReworkProcess.SelectedValue.ToString() == "B1000")
                            {
                                Util.gridClear(dgSelectInput_Prcess);
                            }
                            else
                            {

                                LotTrace(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PROCID").ToString(), cboReworkProcess.SelectedValue.ToString());
                            }
                        }

                        //LOT이력
                        //LotTrace(cboProcessInput.SelectedValue.ToString(), cboReworkProcess.SelectedValue.ToString());
                    }
                    else if (cboRework.SelectedValue.ToString() == "FORM_REWORK")
                    {

                        //대상동을 선택 
                        //재공이동시 동간이동이 가능한지 협의 필요
                        cboTagetArea.IsEnabled = false;
                        cboTagetArea.SelectedValue = DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "AREAID").ToString();
                        cboTagetLine.IsEnabled = false;
                        cboReworkProcess.IsEnabled = false;
                        cboTagetLine.SelectedIndex = 0;
                        cboReworkProcess.SelectedIndex = 0;
                        //LOT이력
                        LotTrace(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PROCID").ToString(), string.Empty);
                    }
                    else if (cboRework.SelectedValue.ToString() == "ASSY_REWORK")
                    {
                        //대상동을 선택 
                        //재공이동시 동간이동이 가능한지 협의 필요
                        cboTagetArea.IsEnabled = true;
                        cboTagetArea.SelectedValue = DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "AREAID").ToString();
                        //대상라인
                        cboTagetLine.SelectedIndex = 0;
                        TagetLine("ASSY_REWORK");
                        cboTagetLine.IsEnabled = true;
                        //이동공정
                        CommonCombo _combo = new CommonCombo();
                        String[] sFilterElectype = { cboTagetLine.SelectedValue.ToString(), "INPUT", cboProcessInput.SelectedValue.ToString(), cboRework.SelectedValue.ToString() };
                        _combo.SetCombo(cboReworkProcess, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "PROCESS_SORT");
                        cboReworkProcess.IsEnabled = true;
                        if (cboReworkProcess.Items.Count == 2)
                        {
                            cboReworkProcess.SelectedIndex = 1;
                        }
                        //LOT이력
                        LotTrace(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PROCID").ToString(), string.Empty);
                    }
                }
                else
                {
                    cboTagetArea.IsEnabled = false;
                    cboTagetLine.IsEnabled = false;
                    cboReworkProcess.IsEnabled = false;
                    cboTagetArea.SelectedIndex = 0;
                    cboTagetLine.SelectedIndex = 0;
                    cboReworkProcess.SelectedIndex = 0;
                }

            }
        }

        private void cboTagetArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgSelectInput.Rows.Count > 0)
            {
                if (cboRework.SelectedValue.ToString() == "MOVE_PROC")
                {
                    TagetLine("MOVE_PROC");
                }
                else if (cboRework.SelectedValue.ToString() == "ASSY_REWORK")
                {
                    TagetLine("ASSY_REWORK");
                }
            }
        }

        //대상라인 선택시
        private void cboTagetLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            if (dgSelectInput.Rows.Count > 0)
            {
                CommonCombo _combo = new CommonCombo();
                if (cboRework.SelectedValue.ToString() == "ASSY_REWORK")
                {
                    String[] sFilterElectype = { cboTagetLine.SelectedValue.ToString(), "INPUT", "", cboRework.SelectedValue.ToString() };
                    _combo.SetCombo(cboReworkProcess, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "PROCESS_SORT");
                    cboReworkProcess.IsEnabled = true;
                    if (cboReworkProcess.Items.Count == 2)
                    {
                        cboReworkProcess.SelectedIndex = 1;
                    }
                }
                else if (cboRework.SelectedValue.ToString() == "MOVE_PROC")
                {

                    String[] sFilterElectype = { cboTagetLine.SelectedValue.ToString(), DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "ROUTID").ToString(), DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "FLOWID").ToString(), DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PROCID").ToString() };
                    _combo.SetCombo(cboReworkProcess, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "TARGET_PROCESS");
                    cboReworkProcess.IsEnabled = true;
                    if (cboReworkProcess.Items.Count == 2)
                    {
                        cboReworkProcess.SelectedIndex = 1;
                    }
                }

            }
            else
            {
                cboReworkProcess.IsEnabled = false;
            }

        }
        //이동공정 선택시 실적삭제공정 조회
        private void cboReworkProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgSelectInput.Rows.Count > 0)
            {
                if (cboReworkProcess.SelectedValue.ToString() == "B1000" || cboReworkProcess.SelectedValue.ToString() == "SELECT")
                {
                    Util.gridClear(dgSelectInput_Prcess);
                }
                else
                {

                    LotTrace(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PROCID").ToString(), cboReworkProcess.SelectedValue.ToString());
                }
            }



        }

        //재공이동
        private void btnReworkMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //DataView dv = (DataView)cboRework.ItemsSource;
                //DataTable dt = dv.Table;
                //DataRow dr = dt.Rows[cboRework.SelectedIndex];

                string _ValueToMessage = string.Empty;
                int StartLenth_Process = cboReworkProcess.Text.Trim().IndexOf(":") + 1;
                int AllLenth_Process = cboReworkProcess.Text.Trim().Length - cboReworkProcess.Text.Trim().IndexOf(":") - 1;

                int StartLenth_Rework = cboRework.Text.Trim().IndexOf(":") + 1;
                int AllLenth_Rework = cboRework.Text.Trim().Length - cboRework.Text.Trim().IndexOf(":") - 1;
                //재공이동 처리 하시겠습니까?\n\n - 이동 공정 : {0}\n\n - 이동 구분 : {1}
                _ValueToMessage = MessageDic.Instance.GetMessage("SFU4164", new object[] { cboReworkProcess.SelectedValue.ToString() == "SELECT" ? string.Empty : cboReworkProcess.Text.ToString().Substring(StartLenth_Process, AllLenth_Process), cboRework.Text.ToString().Substring(StartLenth_Rework, AllLenth_Rework) }); //동일 그룹으로 등록된 조립 LOT이 {0}개 존재합니다. 그래도 등록하시겠습니까?"
                Util.MessageConfirm(_ValueToMessage, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        ReWorkMove();
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void cboProcessInput_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgListInput);
            Util.gridClear(dgSelectInput);
            Util.gridClear(dgSelectInput_Prcess);
            txtRemark.Text = string.Empty;
            cboTagetArea.SelectedIndex = 0;
            cboTagetLine.SelectedIndex = 0;
            cboReworkProcess.SelectedIndex = 0;
            cboRework.SelectedIndex = 0;
        }

        private void btnReSet_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput);
            Util.gridClear(dgSelectInput);
            Util.gridClear(dgSelectInput_Prcess);
            txtRemark.Text = string.Empty;
            cboTagetArea.SelectedIndex = 0;
            cboTagetLine.SelectedIndex = 0;
            cboReworkProcess.SelectedIndex = 0;
            cboRework.SelectedIndex = 0;
        }
        #endregion

        #region [활성화 재공 이동 이력]
        //원각/초소형에 따른 이동코드 재조회
       
        private void txtLotHistory_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (Util.GetCondition(cboReworkHistory, bAllNull: true) == null)
                    {

                        GetHoldHistory_ALL("ONE");
                    }
                    else
                    {
                        GetHoldHistory("ONE");
                    }

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            if (Util.GetCondition(cboReworkHistory, bAllNull: true) == null)
            {
                GetHoldHistory_ALL("ONE");
            }
            else
            {

                GetHoldHistory("ONE");
            }
            //차감공정 콤보 셋팅
             C1.WPF.DataGrid.DataGridComboBoxColumn MinusProcess = dgDetail.Columns["PROCID"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                       
            if (MinusProcess != null)
                MinusProcess.ItemsSource = DataTableConverter.Convert(_dtMinusProcess());
           
        }
        DataTable _dtMinusProcess()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("ASSYPROCID", typeof(string));
            RQSTDT.Columns.Add("LISTCHECK", typeof(string));
            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegmentHistory.SelectedValue.ToString();
            dr["LISTCHECK"] = "Y";
            RQSTDT.Rows.Add(dr);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_SORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            return dtResult;
        }
        #endregion

        #region [대상 선택하기]

        private void dgListInput_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgListInput.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgListInput.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            int seleted_row = dgListInput.CurrentRow.Index;

            ////원각특성/Grade 공정일 경우 저항등급 보이게 하기
            //if(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PROCID").ToString() == Process.CircularCharacteristicGrader)
            //{
            //    dgListInput.Columns["RSST_GRD_NAME"].Visibility = Visibility.Visible;

            //}
            //else
            //{SFU3136
            //    dgListInput.Columns["RSST_GRD_NAME"].Visibility = Visibility.Collapsed;

            //}


            if (DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "CHK").Equals(1))
            {
                // C20221109-000484 : 출고완료된 Pallet
                if (DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "WIPSTAT").Equals("MOVING")
                    && DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "BOX_RCV_ISS_STAT_CODE").Equals("SHIPPING"))
                {
                    string PalletId = String.Empty;
                    PalletId = Convert.ToString(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID"));
                    string _Message = string.Empty;
                    _Message = MessageDic.Instance.GetMessage("SFU3136");

                    ControlsLibrary.MessageBox.Show("Pallet ID : " + PalletId + "\n" + _Message, "", "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            

                            //선택한 PALLETID 행 삭제 
                            DataTable dtTo = DataTableConverter.Convert(dgListInput.ItemsSource);
                            dtTo.Rows.Remove(dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'")[0]);
                            dgListInput.ItemsSource = DataTableConverter.Convert(dtTo);

                            CancelShip(PalletId); // 출고취소
                        }
                        else if(result == MessageBoxResult.Cancel)
                        {
                            DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                        }
                    });
                    return;
                }
                else
                {
                    Seting_dgSelectInput(seleted_row);
                }
            }
            else
            {
                DataTable dtTo = DataTableConverter.Convert(dgSelectInput.ItemsSource);
                if (dtTo.Rows.Count > 0)
                {
                    if (dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'").Length > 0)
                    {
                        dtTo.Rows.Remove(dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'")[0]);
                        dgSelectInput.ItemsSource = DataTableConverter.Convert(dtTo);

                    }
                }
                // 선택된 LOT이 없을 경우 조회조건의 라인정보에 따라 이동구분 셋팅
                if (dtTo.Rows.Count == 0)
                {
                    CommonCombo _combo = new CommonCombo();

                    String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE", "", "" };
                    _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");
                    Util.gridClear(dgSelectInput_Prcess);
                    txtRemark.Text = string.Empty;
                    cboTagetArea.SelectedIndex = 0;
                    cboTagetLine.SelectedIndex = 0;
                    cboReworkProcess.SelectedIndex = 0;
                    cboRework.SelectedIndex = 0;

                    //if (cboEquipmentSegmentInput.SelectedValue != null)
                    //{
                    //    if (cboEquipmentSegmentInput.SelectedValue.ToString() == "M1CF2") // 초소형
                    //    {
                    //        // 초소형 이동구분
                    //        String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE", "CS", String.Empty };
                    //        _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");
                    //    }
                    //    else
                    //    {
                    //        // 원각 이동구분
                    //        String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE","","" };
                    //        _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");
                    //    }
                    //    String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE", "", "" };
                    //    _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");
                    //    Util.gridClear(dgSelectInput_Prcess);
                    //    txtRemark.Text = string.Empty;
                    //    cboTagetArea.SelectedIndex = 0;
                    //    cboTagetLine.SelectedIndex = 0;
                    //    cboReworkProcess.SelectedIndex = 0;
                    //    cboRework.SelectedIndex = 0;
                    //}
                }
            }

        }
        #endregion

        #endregion

        #region Mehod
        #region [재작업 이동]
        public void GetLotList(bool bButton,string sPalletID = "")
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtPalletIdInput).Equals("") && sPalletID.Equals("")) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentInput, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                    dr["PROCID"] = Util.GetCondition(cboProcessInput, bAllNull: true);

                    dr["PJT_NAME"] = txtPrjtNameInput.Text;
                    dr["PRODID"] = txtProdidInput.Text;
                    dr["LOTID_RT"] = txtLotRTDInput.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboQlty, bAllNull: true);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = sPalletID.Equals("") ? Util.GetCondition(txtPalletIdInput) : sPalletID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST_MOVING", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {

                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput);
                    Util.gridClear(dgSelectInput_Prcess);
                    txtRemark.Text = string.Empty;
                    cboTagetArea.SelectedIndex = 0;
                    cboTagetLine.SelectedIndex = 0;
                    cboReworkProcess.SelectedIndex = 0;
                    cboRework.SelectedIndex = 0;

                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletIdInput.Focus();
                            txtPalletIdInput.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        // C20221109-000484 : 출고완료된 Pallet
                        if (Convert.ToString(dtRslt.Rows[0]["WIPSTAT"]).Equals("MOVING") 
                            && Convert.ToString(dtRslt.Rows[0]["BOX_RCV_ISS_STAT_CODE"]).Equals("SHIPPING"))
                        {
                            string PalletId = String.Empty;
                            PalletId = Convert.ToString(dtRslt.Rows[0]["PALLETID"]);
                            string _Message = string.Empty;
                            _Message = MessageDic.Instance.GetMessage("SFU3136"); // 출고취소하시겠습니까?
                            ControlsLibrary.MessageBox.Show("Pallet ID : " + PalletId + "\n" + _Message, "", "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    
                                    CancelShip(PalletId); // 출고취소
                                }
                            });
                        }
                        else
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgListInput.ItemsSource);
                            dtSource.Merge(dtRslt);
                            Util.gridClear(dgListInput);
                            Util.GridSetData(dgListInput, dtSource, FrameOperation, true);
                            DataTableConverter.SetValue(dgListInput.Rows[dgListInput.Rows.Count - 1].DataItem, "CHK", 1);
                            Seting_dgSelectInput(dgListInput.Rows.Count - 1);
                            txtPalletIdInput.Text = string.Empty;
                            txtPalletIdInput.Focus();
                        }
                    }
                    else
                    {

                        Util.gridClear(dgSelectInput);
                        Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    }

                    txtRemark.Text = string.Empty;
                    cboTagetArea.SelectedIndex = 0;
                    cboTagetLine.SelectedIndex = 0;
                    cboReworkProcess.SelectedIndex = 0;
                    cboRework.SelectedIndex = 0;
                }
                else
                {
                    Util.gridClear(dgSelectInput);
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput_Prcess);
                    txtRemark.Text = string.Empty;
                    cboTagetArea.SelectedIndex = 0;
                    cboTagetLine.SelectedIndex = 0;
                    cboReworkProcess.SelectedIndex = 0;
                    cboRework.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// C20221109-000484 : 출고취소
        /// </summary>
        /// <param name="PalletId"></param>
        private void CancelShip(string PalletId)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID");
            inDataTable.Columns.Add("LANGID");
            inDataTable.Columns.Add("NOTE");

            DataRow inDataRow = inDataTable.NewRow();
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataRow["NOTE"] = string.Empty;
            inDataTable.Rows.Add(inDataRow);

            DataTable inBoxTable = indataSet.Tables.Add("INBOX");
            inBoxTable.Columns.Add("BOXID");

            DataRow newRow = inBoxTable.NewRow();
            newRow["BOXID"] = PalletId;
            inBoxTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SHIP_INPALLET_FM", "INDATA,INBOX", null, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    GetLotList(false, PalletId);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }, indataSet);

        }

        private void TagetLine(string MoveCode)
        {

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EXCEPT_ASSY", typeof(string));
                dtRqst.Columns.Add("CHECK_CRCS", typeof(string));
                dtRqst.Columns.Add("CLSS3_CODE", typeof(string)); //MCS: 초소형, MCC: 원형, MCR: 각형
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboTagetArea, bAllNull: true);
                if (MoveCode == "MOVE_PROC")
                {
                    //dr["EXCEPT_ASSY"] = string.Empty;
                    dr["CHECK_CRCS"] = DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "EQSGID");
                    dr["CLSS3_CODE"] = DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "CLSS3_CODE");
                }
                else
                {
                    dr["EXCEPT_ASSY"] = "Y";
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FORM_MOVE_EQUIPMENTSEGMENT_CBO", "INDATA", "OUTDATA", dtRqst);

                cboTagetLine.DisplayMemberPath = "CBO_NAME";
                cboTagetLine.SelectedValuePath = "CBO_CODE";

                DataRow dr2 = dtRslt.NewRow();
                dr2["CBO_NAME"] = "-SELECT-";
                dr2["CBO_CODE"] = "SELECT";
                dtRslt.Rows.InsertAt(dr2, 0);

                cboTagetLine.ItemsSource = DataTableConverter.Convert(dtRslt);
                if (dtRslt.Rows.Count == 2)
                {
                    cboTagetLine.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void LotTrace(string STDProcess, string Process)
        {

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("FR_PROC", typeof(string));
                dtRqst.Columns.Add("TO_PROC", typeof(string));
                dtRqst.Columns.Add("OC_B1000", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "ROUTID");
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                if (cboRework.SelectedValue.ToString() == "MOVE_PROC")
                {
                    if (Process != string.Empty && Process != "SELECT")
                    {
                        if (STDProcess == "B1000") //조회조건의 공정이 포장일 경우
                        {
                            //dr["BOXINGCHK"] = STDProcess;
                            dr["TO_PROC"] = Process;
                        }
                        else
                        {
                            dr["FR_PROC"] = STDProcess;
                            dr["TO_PROC"] = Process;
                        }
                    }

                }
                else
                {

                    if (LoginInfo.CFG_SHOP_ID == "G182")

                    {
                        dr["FR_PROC"] = STDProcess;
                    }
                    else
                    {
                        dr["OC_B1000"] = STDProcess;
                    }


                }
             
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_TRACE", "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgSelectInput_Prcess);

                //C20181121_50523 차감대상 선택 분기
                if (LoginInfo.CFG_SHOP_ID.Equals("A010") && dtRslt != null && dtRqst.Rows.Count > 0)
                {
                    string sFirstPalletId = DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PALLETID").ToString().Substring(0, 1);

                    for(int i=0;i< dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["CHK"] = "False";

                        //2019.12.09    CSR ID : 4077158 이현호 활성화 재고이동 화면에서 이동구분이 ASSY_REWORK(조립 재작업)이며 이동공정이 A4100(X-Ray 재작업)일 경우 실적차감 여부 모두 해제
                        if (cboRework.SelectedValue.ToString() == "ASSY_REWORK" && cboReworkProcess.SelectedValue.ToString() == "A4100")
                        {
                            dtRslt.Rows[i]["CHK"] = "False";
                        }
                        else if (sFirstPalletId.Equals("C") && dtRslt.Rows[i]["PROCID"].ToString().Equals(LGC.GMES.MES.CMM001.Class.Process.CircularCharacteristicGrader))
                        {
                            dtRslt.Rows[i]["CHK"] = "True";
                        }
                        else if (!sFirstPalletId.Equals("C") && !dtRslt.Rows[i]["PROCID"].ToString().Equals(LGC.GMES.MES.CMM001.Class.Process.CircularCharacteristicGrader))
                        {
                            dtRslt.Rows[i]["CHK"] = "True";
                        }

                    }

                }

                Util.GridSetData(dgSelectInput_Prcess, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void ReWorkMove()
        {

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("REWORK_TYPE", typeof(string));
            inDataTable.Columns.Add("TO_AREAID", typeof(string));
            inDataTable.Columns.Add("TO_PROCID", typeof(string));
            inDataTable.Columns.Add("TO_EQSGID", typeof(string));
            inDataTable.Columns.Add("REMARK", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["REWORK_TYPE"] = cboRework.SelectedValue.ToString();
            row["TO_AREAID"] = cboTagetArea.SelectedValue.ToString();
            if (cboReworkProcess.SelectedValue.ToString() != "FORM_REWORK")
            {
                if (cboReworkProcess.SelectedValue.ToString() != "SELECT")
                {
                    row["TO_PROCID"] = cboReworkProcess.SelectedValue.ToString();
                    row["TO_EQSGID"] = cboTagetLine.SelectedValue.ToString();
                }
            }
            row["REMARK"] = txtRemark.Text;
            row["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(row);

            //공정실적 차감 정보
            DataTable inProc = inData.Tables.Add("INPROC");
            inProc.Columns.Add("PROCID", typeof(string));
            inProc.Columns.Add("EQSGID", typeof(string));
            if (dgSelectInput_Prcess.Rows.Count > 0)
            {
                for (int i = 0; i < dgSelectInput_Prcess.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSelectInput_Prcess.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgSelectInput_Prcess.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inProc.NewRow();
                        row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Prcess.Rows[i].DataItem, "PROCID"));
                        row["EQSGID"] = cboTagetLine.SelectedValue.ToString() == "SELECT" ? string.Empty : cboTagetLine.SelectedValue.ToString();
                        inProc.Rows.Add(row);
                    }
                }
            }
            //LOT 정보
            //공정실적 차감 정보
            DataTable inlot = inData.Tables.Add("INLOT");
            inlot.Columns.Add("PALLETID", typeof(string));
            if (dgSelectInput.Rows.Count > 0)
            {
                for (int i = 0; i < dgSelectInput.Rows.Count; i++)
                {
                    row = inlot.NewRow();
                    row["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PALLETID"));
                    inlot.Rows.Add(row);

                }
            }


            try
            {
                //hold 처리
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MOVE_REWORK", "INDATA,INPROC,INLOT", null, inData);
                //이동하였습니다
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Util.gridClear(dgListInput);
                        Util.gridClear(dgSelectInput);
                        Util.gridClear(dgSelectInput_Prcess);

                        txtRemark.Text = string.Empty;
                        cboTagetArea.SelectedIndex = 0;
                        cboTagetLine.SelectedIndex = 0;
                        cboReworkProcess.SelectedIndex = 0;
                        cboRework.SelectedIndex = 0;
                        txtPalletIdInput.Text = string.Empty;
                        GetLotList(false);
                    }
                });
                return;

            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MOVE_REWORK", ex.Message, ex.ToString());

            }
        }

        public void Seting_dgSelectInput(int seleted_row)
        {
            DataTable dtTo = DataTableConverter.Convert(dgSelectInput.ItemsSource);
            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("PALLETID", typeof(string));
                dtTo.Columns.Add("LOTID_RT", typeof(string));
                dtTo.Columns.Add("PJT", typeof(string));
                dtTo.Columns.Add("PRODID", typeof(string));
                dtTo.Columns.Add("WIPQTY", typeof(string));
                dtTo.Columns.Add("UNIT", typeof(string));
                dtTo.Columns.Add("LOTTYPE", typeof(string));
                dtTo.Columns.Add("CALDATE", typeof(string));
                dtTo.Columns.Add("AREAID", typeof(string));
                dtTo.Columns.Add("EQSGID", typeof(string));
                dtTo.Columns.Add("PROCID", typeof(string));
                dtTo.Columns.Add("ROUTID", typeof(string));
                dtTo.Columns.Add("FLOWID", typeof(string));
                dtTo.Columns.Add("S04", typeof(string));
                dtTo.Columns.Add("CLSS3_CODE", typeof(string));
            }
            if (dtTo.Rows.Count > 0)
            {
                //[C20181121_50523] 오창 특성/Grader와 다른 공정인 경우 다중 선택이 불가능 하도록 체크
                if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
                {
                    String sPalletidFirst = DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID").ToString().Substring(0, 1);
                    string sSelectInputPalletFirst = dtTo.Rows[0]["PALLETID"].ToString().Substring(0, 1);
                    Boolean bFirstCharMsg = true;


                    if (sSelectInputPalletFirst.Equals("C") && sPalletidFirst.Equals("C"))
                    {
                        bFirstCharMsg = false;
                    }

                    if ( !sSelectInputPalletFirst.Equals("C") && !sPalletidFirst.Equals("C") )
                    {
                        bFirstCharMsg = false;
                    }

                    if (bFirstCharMsg)
                    {
                        DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);//동일한 동 데이터가 아닙니다.
                        Util.MessageValidation("SFU6001");
                        return;
                    }
                }

                if (dtTo.Select("AREAID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "AREAID") + "'").Length == 0) //동 정보
                {

                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);//동일한 동 데이터가 아닙니다.
                    Util.MessageValidation("SFU4166");
                    return;
                }
                if (dtTo.Select("PROCID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PROCID") + "'").Length == 0) //공정
                {
                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4167");//동일한 공정이 아닙니다.
                    return;
                }
                if (dtTo.Select("ROUTID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "ROUTID") + "'").Length == 0) //ROUIT
                {
                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4168"); //동일한 ROUTE 정보가 아닙니다.
                    return;
                }
                if (dtTo.Select("FLOWID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "FLOWID") + "'").Length == 0) //FLOWID
                {
                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4169"); //동일한 FLOW 정보가 아닙니다.
                    return;
                }
            }
            DataRow dr = dtTo.NewRow();
            foreach (DataColumn dc in dtTo.Columns)
            {
                if (dc.DataType.Equals(typeof(Boolean)))
                {
                    //dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    dr[dc.ColumnName] = 0;
                }
                else
                {
                    dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, dc.ColumnName));
                }
            }
            dtTo.Rows.Add(dr);
            dgSelectInput.ItemsSource = DataTableConverter.Convert(dtTo);

            //선택된 LOT의 라인정보에 따라  이동구분 바인딩
            CommonCombo _combo = new CommonCombo();

          
            if (DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "CLSS3_CODE").ToString() == "MCS") // 초소형
            {
                String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE", "CS", "" };
                _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");
            }
            else
            {
                String[] sFilterElectype = { "", "FORM_LOT_MOVE_TYPE", "", "" };
                _combo.SetCombo(cboRework, CommonCombo.ComboStatus.SELECT, sFilter: sFilterElectype, sCase: "FORM_MOVE_COMMCODES");
            }
            Util.gridClear(dgSelectInput_Prcess);
            txtRemark.Text = string.Empty;
        }

        #endregion
        #region [이력 가져오기]
        public void GetHoldHistory(string palletList)
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRE_PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LOTID_LIST", typeof(string));
                dtRqst.Columns.Add("FORM_LOT_MOVE_TYPE", typeof(string));
                dtRqst.Columns.Add("MOVE_TYPE_ALL_CHK", typeof(string));
                dtRqst.Columns.Add("PRE_PROC_CHK", typeof(string));
                dtRqst.Columns.Add("PROC_CHK", typeof(string));
                dtRqst.Columns.Add("MOVE_PROCID", typeof(string));


                DataRow dr = dtRqst.NewRow();

                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU1499"); // 동을 선택하세요
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                if (dr["EQSGID"].Equals("")) return;
                dr["PRE_PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcessMove, bAllNull: true);

                if (cboProcessHistory.SelectedValue.ToString() != string.Empty)
                {
                    dr["PRE_PROC_CHK"] = "Y";
                }
                if (cboProcessMove.SelectedValue.ToString() != string.Empty)
                {
                    dr["PROC_CHK"] = "Y";
                }
                dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                dr["PRJT_NAME"] = string.IsNullOrWhiteSpace(txtPjtHistory.Text) ? null : txtPjtHistory.Text;
                dr["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotHistory.Text) ? null : txtLotHistory.Text;
                // 다중입력 / 단일입력 검색 분리
                if (palletList.Equals("ONE"))
                {
                    dr["LOTID"] = Util.GetCondition(txtPalletHistory);
                }
                else
                {
                    dr["LOTID_LIST"] = palletList;
                }
                dr["FORM_LOT_MOVE_TYPE"] = Util.GetCondition(cboReworkHistory, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WIP_MOVE_HISTORY", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHistory);
                Util.gridClear(dgDetail);
                ClearValue();
                //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetHoldHistory_ALL(string palletList)
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRE_PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FORM_LOT_MOVE_TYPE", typeof(string));
                dtRqst.Columns.Add("LOTID_LIST", typeof(string));

                //dtRqst.Columns.Add("MOVE_PROCID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU1499"); // 동을 선택하세요
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                if (dr["EQSGID"].Equals("")) return;
                dr["PRE_PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcessMove, bAllNull: true);
                dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                dr["PRJT_NAME"] = string.IsNullOrWhiteSpace(txtPjtHistory.Text) ? null : txtPjtHistory.Text;
                dr["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotHistory.Text) ? null : txtLotHistory.Text;
                // 다중입력 / 단일입력 검색 분리
                if (palletList.Equals("ONE"))
                {
                    dr["LOTID"] = Util.GetCondition(txtPalletHistory);
                }
                else
                {
                    dr["LOTID_LIST"] = palletList;
                }
                dr["FORM_LOT_MOVE_TYPE"] = Util.GetCondition(cboReworkHistory, bAllNull: true);
                //dr["MOVE_PROCID"] = Util.GetCondition(cboProcessMove, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WIP_MOVE_HISTORY_ALL", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHistory);
                Util.gridClear(dgDetail);
                ClearValue();
                //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void dgLotDetail_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        dgListHistory.SelectedIndex = idx;
                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("LANGID", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));
                       
                        DataRow dr = dtRqst.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "PALLETID"));
                    
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_MOVE_HISTORY_DETAIL", "INDATA", "OUTDATA", dtRqst);

                        Util.gridClear(dgDetail);
                        //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);

                        Util.GridSetData(dgDetail, dtRslt, FrameOperation, true);

                        RowCount = dgDetail.Rows.Count;
                        _ACTDTTM = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "ACTDTTM"));
                        _CALDATE = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "CALDATE"));
                        _LOTID = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "PALLETID"));
                        _PRODID = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "PRODID"));
                        _WIPQTY2_ED = Convert.ToDecimal("-" + Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "WIPQTY")));
                        _EQSGID = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "PRE_EQSGID"));
                        _MOVE_TYPE = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "FORM_MOVE_COMMCODES"));
                        _FROM_PROCESS = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "PRE_PROCID"));
                        _TO_PROCESS = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "AFTERMOVE_PROCESS_CODE"));
                        if (_MOVE_TYPE == "ASSY_REWORK")
                        {
                            _ROUTID = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "PRE_ROUTID"));
                        }
                        else
                        {
                            _ROUTID = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "ROUTID"));
                        }
                        _FLOWID =  Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "FLOWID"));
                        if (_TO_PROCESS != "B1000")
                        {
                            SetMinusProcess();
                        }
                  }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }
        #endregion


        #endregion

        #region [Validation]

        #region [재공이동]
        private bool Validation()
        {
            if (cboRework.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4170"); //이동구분을 선택하세요.
                return false;
            }

            // 활성화투입은 공정 정보가 없으므로 체크 Validation 체크 않암
            if (cboRework.SelectedValue.ToString() != "FORM_REWORK")
            {
                if (cboTagetArea.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4171"); //대상동을 선택하세요.
                    return false;
                }
                if (cboTagetLine.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4172"); //대상라인 선택하세요.
                    return false;
                }
                if (cboReworkProcess.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4173"); //대상공정을 선택하세요.
                    return false;
                }
            }



            return true;
        }

        private bool RowAddValidation()
        {
            int ChkProcessCount = 0;
            if (_MinusProcess == null)
            {
                ChkProcessCount = 0;
            }
            else
            {
                ChkProcessCount = _MinusProcess.Rows.Count;
            }
            int ChkRowCount = RowCount + ChkProcessCount;

            if(dgDetail.Rows.Count == ChkRowCount)
            {
                Util.MessageValidation("SFU4214"); //차감실적 추가 불가합니다.
                return false;
            }



            return true;
        }

        private bool MinusProcessDeleteValidation()
        {
            DataRow[] drchk = DataTableConverter.Convert(dgDetail.ItemsSource).Select("CHK = 1");

            if (drchk.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }
            for(int i=0; i< dgDetail.Rows.Count; i++)
            {
                if(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")) == "1" && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "ISREADONLY")) == "False")
                {
                    Util.MessageValidation("SFU4215"); //저장되지 되지 않은 데이터 입니다.
                    return false;
                }
            }

            return true;
        }


        private bool MinusProcessSaveValidation()
        {
            DataRow[] drchk = DataTableConverter.Convert(dgDetail.ItemsSource).Select("CHK = 1");

            if (drchk.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }
            int DubCheck = 0;
            for (int i = 0; i < dgDetail.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")) == "1" && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "ISREADONLY")) == "True")
                {
                    Util.MessageValidation("SFU4216"); //이미 저장된 데이터입니다.
                    return false;
                }
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")) == "1")
                {
                    for(int j =0; j < dgDetail.Rows.Count; j++)
                    {
                        if(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PROCID")) == Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[j].DataItem, "PROCID")))
                        {
                            DubCheck = DubCheck + 1;
                          
                        }
                    }
                    if (DubCheck > 1)
                    {
                        Util.MessageValidation("SFU4217"); //동일한 공정이 선택되었습니다.
                        return false;
                    }
                    else
                    {
                        DubCheck = 0;
                    }
                }
            }
           
            return true;
        }

        #endregion

        #endregion


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {


            if (!RowAddValidation())
            {
                return;
            }
            DataTable dtSource = DataTableConverter.Convert(dgDetail.ItemsSource);

            DataRow dr = dtSource.NewRow();
            dr["CHK"] = 1;
            dr["WIPSEQ"] = System.DBNull.Value;
            dr["ACTDTTM"] = _ACTDTTM;
            dr["CALDATE"] = _CALDATE;
            dr["LOTID"] = _LOTID;
            dr["PRODID"] = _PRODID;
            dr["PROCID"] = string.Empty;
            dr["PROCNAME"] = string.Empty;
            dr["WIPQTY2_ED"] = _WIPQTY2_ED;
            dr["INSUSER"] = string.Empty;
            dr["USERNAME"] = string.Empty;
            dr["EQSGID"] = _EQSGID;
            dr["ISREADONLY"] = "False";
            dtSource.Rows.Add(dr);
            Util.gridClear(dgDetail);
            Util.GridSetData(dgDetail, dtSource, FrameOperation, true);
        }

        public void ClearValue()
        {
            _ACTDTTM = string.Empty;
            _CALDATE = string.Empty;
            _LOTID = string.Empty;
            _PRODID = string.Empty;
            _EQSGID = string.Empty;
            _MOVE_TYPE = string.Empty;
            _FROM_PROCESS = string.Empty;
            _TO_PROCESS = string.Empty;
            _WIPQTY2_ED = 0;
        }

        public void SetMinusProcess()
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("FR_PROC", typeof(string));
                dtRqst.Columns.Add("TO_PROC", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = _ROUTID;

                if (_MOVE_TYPE == "MOVE_PROC")
                {
                    
                    if (_FROM_PROCESS == "B1000") //조회조건의 공정이 포장일 경우
                    {
                         dr["TO_PROC"] = _TO_PROCESS;
                    }
                    else
                    {
                        dr["FR_PROC"] = _FROM_PROCESS;
                        dr["TO_PROC"] = _TO_PROCESS;
                    }
                   

                }
                else
                {
                    dr["FR_PROC"] = _FROM_PROCESS;
                }
                dr["LOTID"] = _LOTID;
                dtRqst.Rows.Add(dr);

                //DataTable dtRqst = new DataTable();
                // dtRqst.Columns.Add("LANGID", typeof(string));
                // dtRqst.Columns.Add("EQSGID", typeof(string));
                // dtRqst.Columns.Add("FROM_PROCESS", typeof(string));
                // dtRqst.Columns.Add("TO_PROCESS", typeof(string));
                // dtRqst.Columns.Add("LOTID", typeof(string));
                // dtRqst.Columns.Add("FORM_MOVE_CHK", typeof(string));
                // dtRqst.Columns.Add("FROM_PROCESS_CHK", typeof(string));
                // DataRow dr = dtRqst.NewRow();

                // dr["LANGID"] = LoginInfo.LANGID;
                // dr["EQSGID"] = _EQSGID;
                // dr["FROM_PROCESS"] = _FROM_PROCESS;
                // dr["TO_PROCESS"] = _TO_PROCESS;
                // dr["LOTID"] = _LOTID;
                // if (_MOVE_TYPE == "MOVE_PROC")
                // {
                //    dr["FORM_MOVE_CHK"] = "Y";
                // }
                // else
                // {
                //     dr["FORM_MOVE_CHK"] = null;
                // }
                // if(_FROM_PROCESS == "B1000")
                // {
                //     dr["FROM_PROCESS_CHK"] = null;
                // }
                // else
                // {
                //     dr["FROM_PROCESS_CHK"] = "Y";
                // }

                // dtRqst.Rows.Add(dr);
                _MinusProcess = new DataTable();
                _MinusProcess = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WIP_MOVE_PROCESS", "INDATA", "OUTDATA", dtRqst);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgDetail_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;
            if (cbo != null)
            {
                if (e.Column.Name == "PROCID")
                {
                   cbo.ItemsSource = DataTableConverter.Convert(_MinusProcess);
                   cbo.SelectedIndex = 0;
                }
            }

        }
        private void dgDetail_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "PROCID")
            {
                if (DataTableConverter.GetValue(dgDetail.Rows[e.Row.Index].DataItem, "ISREADONLY").ToString() == "True")
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void btnSaveDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MinusProcessSaveValidation())
                {
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DetailSave();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDeleteDtail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MinusProcessDeleteValidation())
                {
                    return;
                }

                //삭제하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DetailDelete();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void DetailSave()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
             DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;
            row["LOTID"] = _LOTID;
            row["AREAID"] = _EQSGID.Substring(0,2);

            inDataTable.Rows.Add(row);

            //저장될 공정정보
            DataTable inProc = inData.Tables.Add("INPROC");
            inProc.Columns.Add("ACTQTY", typeof(Decimal));
            inProc.Columns.Add("PROCID", typeof(string));
            inProc.Columns.Add("CALDATE", typeof(string));

            for (int i = 0; i < dgDetail.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inProc.NewRow();
                    row["ACTQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPQTY2_ED")));
                    row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PROCID"));
                    row["CALDATE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CALDATE"));
                    inProc.Rows.Add(row);
                }
            }
            try
            {
                //차감공정저장
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_REWORK_PROD_QTY", "INDATA,INPROC", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("LANGID", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));
                        DataRow dr = dtRqst.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["LOTID"] = _LOTID;
                        dtRqst.Rows.Add(dr);
                        RowCount = dgDetail.Rows.Count;
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_MOVE_HISTORY_DETAIL", "INDATA", "OUTDATA", dtRqst);
                        Util.gridClear(dgDetail);
                        Util.GridSetData(dgDetail, dtRslt, FrameOperation, true);
                        if (_TO_PROCESS != "B1000")
                        {
                            SetMinusProcess();
                        }
                    });
                    return;

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MODIFY_REWORK_PROD_QTY", ex.Message, ex.ToString());
            }
        }

        private void DetailDelete()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;
            row["LOTID"] = _LOTID;
            row["AREAID"] = _EQSGID.Substring(0, 2);

            inDataTable.Rows.Add(row);

            //저장될 공정정보
            DataTable inProc = inData.Tables.Add("INPROC");
            inProc.Columns.Add("ACTQTY", typeof(Decimal));
            inProc.Columns.Add("PROCID", typeof(string));
            inProc.Columns.Add("WIPSEQ", typeof(Decimal));

            for (int i = 0; i < dgDetail.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inProc.NewRow();
                    row["ACTQTY"] = 0;
                    row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PROCID"));
                    row["WIPSEQ"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")));
                    inProc.Rows.Add(row);
                }
            }
            try
            {
                //차감공정저장
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_REWORK_PROD_QTY", "INDATA,INPROC", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("LANGID", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));
                        DataRow dr = dtRqst.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["LOTID"] = _LOTID;
                        dtRqst.Rows.Add(dr);
                        RowCount = dgDetail.Rows.Count;
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_MOVE_HISTORY_DETAIL", "INDATA", "OUTDATA", dtRqst);
                        Util.gridClear(dgDetail);
                        Util.GridSetData(dgDetail, dtRslt, FrameOperation, true);
                        if (_TO_PROCESS != "B1000")
                        {
                            SetMinusProcess();
                        }
                    });
                    return;

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MODIFY_REWORK_PROD_QTY", ex.Message, ex.ToString());
            }
        }

        private void btnSearchHistoryCancel_Click(object sender, RoutedEventArgs e)
        {
            GetHistoryCancelList();
        }

        private void GetHistoryCancelList()
        {
            if (Util.GetCondition(cboReworkHistoryCancel, bAllNull: true) == null)
            {
                GetHistoryCancel_ALL("ONE");
            }
            else
            {

                GetHistoryCancel("ONE");
            }
        }

        public void GetHistoryCancel(string palletList)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRE_PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LOTID_LIST", typeof(string));
                dtRqst.Columns.Add("FORM_LOT_MOVE_TYPE", typeof(string));
                dtRqst.Columns.Add("MOVE_TYPE_ALL_CHK", typeof(string));
                dtRqst.Columns.Add("PRE_PROC_CHK", typeof(string));
                dtRqst.Columns.Add("PROC_CHK", typeof(string));
                dtRqst.Columns.Add("MOVE_PROCID", typeof(string));
                dtRqst.Columns.Add("CANCEL_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHistCancel);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHistCancel);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHistoryCancel, "SFU1499"); // 동을 선택하세요
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistoryCancel, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                if (dr["EQSGID"].Equals("")) return;
                dr["PRE_PROCID"] = Util.GetCondition(cboProcessHistoryCancel, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcessMoveCancel, bAllNull: true);

                if (cboProcessHistoryCancel.SelectedValue.ToString() != string.Empty)
                {
                    dr["PRE_PROC_CHK"] = "Y";
                }
                if (cboProcessMoveCancel.SelectedValue.ToString() != string.Empty)
                {
                    dr["PROC_CHK"] = "Y";
                }
                dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdIDCancel.Text) ? null : txtProdIDCancel.Text;
                dr["PRJT_NAME"] = string.IsNullOrWhiteSpace(txtPjtHistoryCancel.Text) ? null : txtPjtHistoryCancel.Text;
                dr["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotHistoryCancel.Text) ? null : txtLotHistoryCancel.Text;
                
                // 다중입력 / 단일입력 검색 분리
                if (palletList.Equals("ONE"))
                {
                    dr["LOTID"] = Util.GetCondition(txtPalletHistoryCancel);
                }
                else
                {
                    dr["LOTID_LIST"] = palletList;
                }
                dr["FORM_LOT_MOVE_TYPE"] = Util.GetCondition(cboReworkHistoryCancel, bAllNull: true);
                dr["CANCEL_FLAG"] = "N";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WIP_MOVE_HISTORY", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHistoryCancel);
                Util.GridSetData(dgListHistoryCancel, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetHistoryCancel_ALL(string palletList)
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRE_PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LOTID_LIST", typeof(string));
                dtRqst.Columns.Add("FORM_LOT_MOVE_TYPE", typeof(string));
                dtRqst.Columns.Add("CANCEL_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHistCancel);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHistCancel);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHistoryCancel, "SFU1499"); // 동을 선택하세요
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistoryCancel, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                if (dr["EQSGID"].Equals("")) return;
                dr["PRE_PROCID"] = Util.GetCondition(cboProcessHistoryCancel, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcessMoveCancel, bAllNull: true);
                dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdIDCancel.Text) ? null : txtProdIDCancel.Text;
                dr["PRJT_NAME"] = string.IsNullOrWhiteSpace(txtPjtHistoryCancel.Text) ? null : txtPjtHistoryCancel.Text;
                dr["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotHistoryCancel.Text) ? null : txtLotHistoryCancel.Text;
                // 다중입력 / 단일입력 검색 분리
                if (palletList.Equals("ONE"))
                {
                    dr["LOTID"] = Util.GetCondition(txtPalletHistoryCancel);
                }
                else
                {
                    dr["LOTID_LIST"] = palletList;
                }
                dr["FORM_LOT_MOVE_TYPE"] = Util.GetCondition(cboReworkHistoryCancel, bAllNull: true);
                dr["CANCEL_FLAG"] = "N";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WIP_MOVE_HISTORY_ALL", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHistoryCancel);
                Util.GridSetData(dgListHistoryCancel, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtLotHistoryCancel_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetHistoryCancelList();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnHistoryCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationCancel())
                {
                    return;
                }

                //처리 하시겠습니까?
                Util.MessageConfirm("SFU1925", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        ReWorkMoveCancel();
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidationCancel()
        {
            DataRow[] drchk = DataTableConverter.Convert(dgListHistoryCancel.ItemsSource).Select("CHK = 1");

            if (drchk.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (drchk.Length > 10)
            {
                // 한번에 최대 %1개 까지 처리 가능합니다.
                Util.MessageValidation("SFU5015", "10");
                return false;
            }

            return true;
        }

        private void ReWorkMoveCancel()
        {

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(row);

            //취소할 이동 정보
            DataTable inLotTable = inData.Tables.Add("INLOT");
            inLotTable.Columns.Add("REWORK_TYPE", typeof(string));
            inLotTable.Columns.Add("PALLETID", typeof(string));
            inLotTable.Columns.Add("TO_PROCID", typeof(string));
            inLotTable.Columns.Add("TO_EQSGID", typeof(string));
            inLotTable.Columns.Add("FROM_PROCID", typeof(string));
            inLotTable.Columns.Add("FROM_EQSGID", typeof(string));
            inLotTable.Columns.Add("ACTDTTM", typeof(string));
            inLotTable.Columns.Add("WIPQTY", typeof(decimal));

            if (dgListHistoryCancel.Rows.Count > 0)
            {
                for (int inx = 0; inx < dgListHistoryCancel.Rows.Count; inx++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "CHK")).Equals("1"))
                    {
                        row = inLotTable.NewRow();
                        row["REWORK_TYPE"] = Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "FORM_MOVE_COMMCODES"));
                        row["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "PALLETID"));
                        row["FROM_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "AFTERMOVE_PROCESS_CODE"));
                        row["FROM_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "AFTERMOVE_EQSGID"));
                        row["TO_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "PRE_PROCID"));
                        row["TO_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "PRE_EQSGID"));
                        row["ACTDTTM"] = Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "ACTDTTM"));
                        row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgListHistoryCancel.Rows[inx].DataItem, "WIPQTY"));
                        inLotTable.Rows.Add(row);
                    }
                }
            }

            try
            {
                //hold 처리
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MOVE_REWORK_CANCEL", "INDATA,INLOT", null, inData);

                //이동하였습니다
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        GetHistoryCancelList();
                    }
                });
                return;

            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MOVE_REWORK_CANCEL", ex.Message, ex.ToString());

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

        private void txtPalletHistory_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();


                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }
                    string palletList = String.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;
                        }
                        palletList += sPasteStrings[i].Trim()+",";
                    }


                    if (Util.GetCondition(cboReworkHistory, bAllNull: true) == null)
                    {

                        GetHoldHistory_ALL(palletList);
                    }
                    else
                    {
                        GetHoldHistory(palletList);
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

        private void txtPalletHistoryCancel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();


                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }
                    string palletList = String.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;
                        }
                        palletList += sPasteStrings[i].Trim() + ",";
                    }


                    if (Util.GetCondition(cboReworkHistoryCancel, bAllNull: true) == null)
                    {
                        GetHistoryCancel_ALL(palletList);
                    }
                    else
                    {
                        GetHistoryCancel(palletList);
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
    }
}
