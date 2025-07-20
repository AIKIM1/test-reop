
/*************************************************************************************
 Created Date : 2023.11.15
      Creator : 배현우
   Decription : 포트별 자재 설정 (전극 믹서 자재 설정(공정별) 참고하여 생성)
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.15  배현우 : Initial Created.(Copy by ELEC001_027_CNB)
  2024.01.16  배현우 : 설비조건 ALL 추가 및 설비ID, 설비명, 포트명 컬럼 추가
  2024.06.20  최평부 : 1. 회전여부,버퍼수,투입수량 항목 추가, 2. 화면 : 설비combo, 등등 BR 분기처리
  2024.08.26  최평부 : 조회조건 LINE COMBO 추가(공통코드 : DIFFUSION_SITE 로 SITE 별 분기처리)
  2025.02.11  김준형 : 설비명 컬럼 삭제/ 매핑삭제 버튼 추가 / 자재명 컬럼 추가(공통코드 : DIFFUSION_SITE 로 SITE 별 분기처리하여 ESST만 보여지도록 함.)
  2025.04.09  김건식 : 투입포트 탭 추가
  2025.04.09  유현민 : 투입포트 탭 수정
  2025.05.09  유현민 : 투입포트 탭 수정
  2025.06.25  김홍기 : Pack 영역 화면 분리
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_051 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        string _ProdID;
        string _ProcChkEqptID;
        bool _DiffusionSiteFlag = false;
        string _Da;
        string _Da2;
        string _Da3;
        string _Da4;
        string _EQSGID;
        string INPORT_EQSGID;

        public PACK003_051()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitCombo();


            this.Loaded -= UserControl_Loaded;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRegister);
            listAuth.Add(btnUnMapping);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);


        }

        #endregion
        private void InitCombo()
        {
            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            tabInPortSet.Visibility = Visibility.Collapsed;

            //2024-06-20 by 최평부
            //diffusion_site 공통코드 조회(화면 : 설비combo, 등등 BR 분기처리)
            DataTable dtDiffusionSite = new DataTable();
            dtDiffusionSite = GetCommonCode("DIFFUSION_SITE", "AUTO_LOGIS");

            string area_id = string.Empty;
            _Da = "DA_BAS_SEL_EQUIPMENT_CBO";
            _Da2 = "DA_BAS_SEL_TB_SFC_PORT_MTRL_SET";
            _Da3 = "DA_PRD_SEL_MTRL_INFO_WITH_FP";
            _Da4 = "DA_BAS_SEL_EQUIPMENT_CBO";  //임시. 공정없어서 조회안됨

            _EQSGID = LoginInfo.CFG_EQSG_ID;
            INPORT_EQSGID = LoginInfo.CFG_EQSG_ID;

            //라인
            //combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: Filter);
            //라인 2024.08.26 BY 최평부
            string[] arrColumn0 = { "LANGID", "AREAID" };
            string[] arrCondition0 = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboEquipmentSegment.SetDataComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", arrColumn0, arrCondition0, CommonCombo.ComboStatus.SELECT, false);
            cboEquipmentSegment.SelectedValue = _EQSGID;
            if (cboEquipmentSegment.SelectedIndex < 0) cboEquipmentSegment.SelectedIndex = 0;

            // 투입 PORT TAB 라인
            cboInPortEquipmentSegment.SetDataComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", arrColumn0, arrCondition0, CommonCombo.ComboStatus.SELECT, false);
            cboInPortEquipmentSegment.SelectedValue = _EQSGID;
            if (cboInPortEquipmentSegment.SelectedIndex < 0) cboInPortEquipmentSegment.SelectedIndex = 0;

            //2024-06-20 by 최평부
            //diffusion_site 공통코드 조회(화면 : 설비combo, 등등 BR 분기처리)
            if (dtDiffusionSite.Rows.Count > 0)
            {
                area_id = dtDiffusionSite.Rows[0]["ATTRIBUTE2"].ToString();

                if (area_id.Contains(LoginInfo.CFG_AREA_ID))
                {
                    _Da = "DA_BAS_SEL_EQUIPMENT_CBO_DIFFUSION";
                    _Da2 = "DA_BAS_SEL_TB_SFC_PORT_MTRL_SET_DIFFUSION";
                    //_Da3 = "DA_PRD_SEL_MTRL_INFO_WITH_FP_DIFFUSION";
                    _Da3 = "BR_PRD_SEL_MTRL_INFO_WITH_FP_DIFFUSION";  // 자재타입 컬럼 공통코드명 조회를 위한 호출 biz 변경 DA->BR 250124 김준형
                    _Da4 = "DA_BAS_SEL_EQUIPMENT_CBO_DIFFUSION_NOTIN_PRCSEQP";

                    tb_01.Visibility = Visibility.Visible;
                    tb_02.Visibility = Visibility.Visible;
                    col_01.Width = GridLength.Auto;

                    sp_01.Margin = new Thickness(50, 0, 8, 0);
                    sp_02.Margin = new Thickness(50, 0, 8, 0);

                    cboEquipmentSegment.Visibility = Visibility.Visible;

                    btnUnMapping.Visibility = Visibility.Collapsed;
                    btnDelMapping.Visibility = Visibility.Visible;

                    _EQSGID = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    INPORT_EQSGID = Convert.ToString(cboInPortEquipmentSegment.SelectedValue);

                    tabInPortSet.Visibility = Visibility.Visible;

                    _DiffusionSiteFlag = true;


                }
            }
            // 투입 PORT TAB 설비
            string[] arrColumn3 = { "LANGID", "EQSGID" };
            string[] arrCondition3 = { LoginInfo.LANGID, INPORT_EQSGID };
            cboInPortEquipment.SetDataComboItem(_Da4, arrColumn3, arrCondition3, CommonCombo.ComboStatus.ALL, false);
            cboInPortEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
            if (cboInPortEquipment.SelectedIndex < 0) cboInPortEquipment.SelectedIndex = 0;

            //공정           
            string[] arrColumn1 = { "LANGID", "EQSGID" };
            string[] arrCondition1 = { LoginInfo.LANGID, _EQSGID }; // 2024.08.26 BY 최평부
            cboProcess.SetDataComboItem("DA_BAS_SEL_PROCESS_CBO", arrColumn1, arrCondition1, _DiffusionSiteFlag == true ? CommonCombo.ComboStatus.ALL : CommonCombo.ComboStatus.SELECT, false);
            cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;
            if (cboProcess.SelectedIndex < 0) cboProcess.SelectedIndex = 0;

            //설비
            string[] arrColumn2 = { "LANGID", "EQSGID", "PROCID" };
            string[] arrCondition2 = { LoginInfo.LANGID, _EQSGID, cboProcess.GetStringValue() };
            cboEquipment.SetDataComboItem(_Da, arrColumn2, arrCondition2, CommonCombo.ComboStatus.ALL, false);
            cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
            if (cboEquipment.SelectedIndex < 0) cboEquipment.SelectedIndex = 0;
        }

        //2024.08.27 BY 최평부 추가
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _EQSGID = cboEquipmentSegment.GetStringValue();
            //공정
            string[] arrColumn2 = { "LANGID", "EQSGID" };
            string[] arrCondition2 = { LoginInfo.LANGID, cboEquipmentSegment.GetStringValue() };
            cboProcess.SetDataComboItem("DA_BAS_SEL_PROCESS_CBO", arrColumn2, arrCondition2, CommonCombo.ComboStatus.ALL, false);

            Util.gridClear(dgBatchOrder);
            Util.gridClear(dgMaterial);
        }


        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //설비
            string[] arrColumn2 = { "LANGID", "EQSGID", "PROCID" };
            //2024.08.26 라인 정보 분기처리
            string[] arrCondition2 = { LoginInfo.LANGID, _EQSGID, cboProcess.GetStringValue() };
            cboEquipment.SetDataComboItem(_Da, arrColumn2, arrCondition2, CommonCombo.ComboStatus.ALL, false);
            GetEqptPort();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender != null) GetEqptPort();

            GetBachOrder(dgBatchOrder, dgMaterial);
        }

        private void dgPortChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("False")))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dg.SelectedIndex = idx;
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (cboEquipment.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1673"); //설비를 선택하세요
                return;
            }

            GetEqptPort();
            GetBachOrder(dgBatchOrder, dgMaterial);

        }

        private void GetEqptPort()
        {
            try
            {
                //if (cboEquipment.GetBindValue() == null) return;

                //2024-06-20 by 최평부
                //회전여부,버퍼수,투입수량 항목 추가
                if (_DiffusionSiteFlag)
                {
                    dgEqptPort.Columns["PRDT_SPEC_CHK_FLAG"].Visibility = Visibility.Visible;
                    dgEqptPort.Columns["LOT_PROD_MAX_QTY"].Visibility = Visibility.Visible;
                    dgEqptPort.Columns["EQPT_NAME"].Visibility = Visibility.Collapsed;
                }

                Util.gridClear(dgEqptPort);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("EQPT_GROUP_ID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = _EQSGID;//2024.08.26 by 최평부 : 라인 정보 사이트별 분기처리

                // 2024.08.26 BY 최평부
                if (_DiffusionSiteFlag)
                {
                    row["PROCID"] = cboProcess.Text.Equals("ALL") ? null : Convert.ToString(cboProcess.SelectedValue);
                }
                else
                {
                    row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                }
                row["EQPTID"] = cboEquipment.Text.Equals("ALL") ? null : Convert.ToString(cboEquipment.SelectedValue);
                row["EQPT_GROUP_ID"] = GetTabGroupName();
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync(_Da2, "RQSTDT", "RSLTDT", dt);
                Util.GridSetData(dgEqptPort, result, FrameOperation, true);
                dgEqptPort.MergingCells -= dgEqptPort_MergingCells;
                dgEqptPort.MergingCells += dgEqptPort_MergingCells;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetBachOrder(C1DataGrid dgBatchOrder, C1DataGrid dgMaterial)
        {
            try
            {
                bool isProc = false;
                dgBatchOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;




                Util.gridClear(dgBatchOrder);
                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (this.tabControlMain.SelectedIndex.Equals(0))
                {
                    Indata["EQSGID"] = _EQSGID;//2024.08.26 by 최평부 : 라인 정보 사이트별 분기처리
                    Indata["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                    Indata["PROCID"] = cboProcess.GetBindValue();

                    Indata["STDT"] = Convert.ToDateTime(ldpDatePickerFrom.SelectedDateTime).ToString("yyyyMMdd");
                    Indata["EDDT"] = Convert.ToDateTime(ldpDatePickerTo.SelectedDateTime).ToString("yyyyMMdd");
                }
                else
                {
                    Indata["EQSGID"] = null;  //INPORT_EQSGID;  전체조회로 변경(공통라인 사용으로 인한 변경)
                    Indata["EQPTID"] = Convert.ToString(cboInPortEquipment.SelectedValue);
                    Indata["PROCID"] = null;

                    Indata["STDT"] = Convert.ToDateTime(ldpInPortDPFrom.SelectedDateTime).ToString("yyyyMMdd");
                    Indata["EDDT"] = Convert.ToDateTime(ldpInPortDPTo.SelectedDateTime).ToString("yyyyMMdd");
                }


                IndataTable.Rows.Add(Indata);

                //2024.07 BY 최평부 조회로직 분기 처리
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WO_MTRL_REL_COM", "INDATA", "RSLTDT", IndataTable);
                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_MTRL_REL_COM", "INDATA", "RSLTDT", IndataTable);

                dgBatchOrder.BeginEdit();

                Util.GridSetData(dgBatchOrder, dtMain, FrameOperation, true);
                dgBatchOrder.EndEdit();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
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

        private void dgEqptPort_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (dgEqptPort.Rows.Count <= 0)
                {
                    return;
                }
                int x = 0;
                int x1 = 0;
                for (int i = x1; i < dgEqptPort.GetRowCount(); i++)
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dgEqptPort.GetCell(x, dgEqptPort.Columns["EQPTID"].Index).Value)))
                    {
                        if (Util.NVC(dgEqptPort.GetCell(x, dgEqptPort.Columns["EQPTID"].Index).Value) == Util.NVC(dgEqptPort.GetCell(i, dgEqptPort.Columns["EQPTID"].Index).Value))
                        {
                            x1 = i;
                        }
                        else
                        {
                            for (int j = 1; j < dgEqptPort.Columns.Count - 6; j++)
                            {
                                // 2024.08.26 BY 최평부
                                if (j != 3 && j != 4)
                                {
                                    e.Merge(new DataGridCellsRange(dgEqptPort.GetCell((int)x, (int)j), dgEqptPort.GetCell((int)x1, (int)j)));
                                }
                            }

                            x = x1 + 1;
                            i = x1;
                        }
                    }

                }
                for (int j = 1; j < dgEqptPort.Columns.Count - 6; j++)
                {
                    //최평부 추가 MERGE 컬럼 구분 2024.07.11
                    if (j != 3 && j != 4)
                    {
                        e.Merge(new DataGridCellsRange(dgEqptPort.GetCell((int)x, (int)j), dgEqptPort.GetCell((int)x1, (int)j)));
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            GetBachOrder(dgBatchOrder, dgMaterial);
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            GetBachOrder(dgBatchOrder, dgMaterial);
        }

        private void dgBatchOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dg.SelectedIndex = idx;

                    _ProcChkEqptID = string.Empty;
                    //_ProdID = Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "PRODID"));
                    //GetMaterial(Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "WOID")));
                    string sProdID = Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "PRODID"));
                    string sWOID = Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "WOID"));

                    GetMaterial(sWOID, sProdID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgMaterialChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1")
                                       || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("True") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("False")) // MES 2.0 CHK 컬럼 오류 Patch
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dg.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetMaterial(string WOID, string PRODID)
        {
            try
            {
                //2024-06-20 by 최평부
                //회전여부,버퍼수,투입수량 항목 추가
                if (_DiffusionSiteFlag)
                {
                    // 25-01-23 김준형 자재타입 추가
                    dgMaterial.Columns["MTRLTYPE"].Visibility = Visibility.Visible;
                    dgMaterial.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                }

                if (this.tabControlMain.SelectedIndex.Equals(0))
                {
                    Util.gridClear(dgMaterial);
                }
                else
                {
                    Util.gridClear(dgInPortMaterial);
                }

                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));

                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.AcceptChanges();

                DataRow Indata = IndataTable.NewRow();

                if (!string.IsNullOrEmpty(WOID))
                {
                    Indata["WOID"] = WOID;
                }

                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                if (this.tabControlMain.SelectedIndex.Equals(0))
                {
                    Indata["EQSGID"] = _EQSGID;//2024.08.26 by 최평부 : 라인 정보 사이트별 분기처리;
                }
                else
                {
                    Indata["EQSGID"] = null;
                }

                Indata["PRODID"] = PRODID;
                Indata["LANGID"] = LoginInfo.LANGID;

                //Indata["PROCID"] = cboProcess.GetBindValue();

                IndataTable.Rows.Add(Indata);

                //string sBizName = "DA_PRD_SEL_MTRL_INFO_WITH_FP";


                DataTable dtMain = new ClientProxy().ExecuteServiceSync(_Da3, "INDATA", "OUTDATA", IndataTable);
                if (dtMain.Rows.Count == 0)
                {
                    return;
                }

                // 현재 선택한 탭에 따라 뿌려주는 그리드 변경
                if(this.tabControlMain.SelectedIndex.Equals(0))
                {
                    Util.GridSetData(dgMaterial, dtMain, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgInPortMaterial, dtMain, FrameOperation, true);
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string MTRLID = null;
            string WOID = null;
            C1DataGrid dg = null;

            // 현재 선택한 탭에 따라 체그해야 할 그리드 변경
            if (this.tabControlMain.SelectedIndex.Equals(0))
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgBatchOrder, "CHK") == -1)
                {
                    Util.MessageValidation("SFU3522"); //배치오더를 선택해주세요
                    return;
                }
                if (_Util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK") == -1)
                {
                    Util.MessageValidation("SFU3523"); //자재를 선택해주세요
                    return;
                }
                if (_Util.GetDataGridCheckFirstRowIndex(dgEqptPort, "CHK") == -1)
                {
                    Util.MessageValidation("SFU3830"); //포트를 선택해주세요
                    return;
                }

                MTRLID = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK")].DataItem, "MTRLID"));
                WOID = Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBatchOrder, "CHK")].DataItem, "WOID"));
                dg = dgEqptPort;
            }
            else
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgInPortBatchOrder, "CHK") == -1)
                {
                    Util.MessageValidation("SFU3522"); //배치오더를 선택해주세요
                    return;
                }
                if (_Util.GetDataGridCheckFirstRowIndex(dgInPortMaterial, "CHK") == -1)
                {
                    Util.MessageValidation("SFU3523"); //자재를 선택해주세요
                    return;
                }
                if (_Util.GetDataGridCheckFirstRowIndex(dgEqptInPort, "CHK") == -1)
                {
                    Util.MessageValidation("SFU3830"); //포트를 선택해주세요
                    return;
                }

                MTRLID = Util.NVC(DataTableConverter.GetValue(dgInPortMaterial.Rows[_Util.GetDataGridCheckFirstRowIndex(dgInPortMaterial, "CHK")].DataItem, "MTRLID"));
                WOID = Util.NVC(DataTableConverter.GetValue(dgInPortBatchOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(dgInPortBatchOrder, "CHK")].DataItem, "WOID"));
                dg = dgEqptInPort;
            }

            SavePortInfo(dg, MTRLID, WOID, "Y", "SELECT");

        }

        private void btnUnMapping_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgEqptPort, "CHK") == -1)
            {
                Util.MessageValidation("SFU3830"); //포트를 선택해주세요
                return;
            }

            SavePortInfo(dgEqptPort
                      , null
                      , null
                      , "N"
                      , "DELETE");

        }

        private void SavePortInfo(C1DataGrid dg, string mtrlid, string wo_detl_id, string use_flag, string btnType)
        {
            string msg = btnType.Equals("SELECT") ? "SFU1241" : _DiffusionSiteFlag ? "SFU8939" : "SFU3831";  //"저장하시겠습니까? : diffusion_site flag y 인 경우 "매핑해제하시겠습니까?" : "포트를 삭제하시겠습니까?"  

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet ds = new DataSet();
                        DataTable indata = ds.Tables.Add("INDATA");
                        indata.Columns.Add("EQPTID", typeof(string));
                        indata.Columns.Add("PORT_ID", typeof(string));
                        indata.Columns.Add("MTRLID", typeof(string));
                        indata.Columns.Add("WO_DETL_ID", typeof(string));
                        indata.Columns.Add("USE_FLAG", typeof(string));
                        indata.Columns.Add("USERID", typeof(string));

                        DataRow row = indata.NewRow();
                        row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[_Util.GetDataGridCheckFirstRowIndex(dg, "CHK")].DataItem, "EQPTID"));
                        row["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[_Util.GetDataGridCheckFirstRowIndex(dg, "CHK")].DataItem, "PORT_ID"));

                        //2024-06-25 by 최평부
                        //diffusion_site 공통코드 조회 분기처리(삭제 시 자재코드를 삭제하지 않게..)
                        if (btnType.Equals("DELETE") && _DiffusionSiteFlag)
                        {
                            row["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[_Util.GetDataGridCheckFirstRowIndex(dg, "CHK")].DataItem, "MTRLID"));
                        }
                        else
                        {
                            row["MTRLID"] = mtrlid;
                        }

                        row["WO_DETL_ID"] = wo_detl_id;
                        row["USE_FLAG"] = use_flag;
                        row["USERID"] = LoginInfo.USERID;

                        indata.Rows.Add(row);


                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PORT_MTRL_SET", "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");//정상처리되었습니다.

                                if (this.tabControlMain.SelectedIndex.Equals(0))
                                {
                                    GetEqptPort();
                                }
                                else
                                {
                                    GetEqptInPort();
                                }

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }



        private void dgEqptPort_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Presenter == null) return;


        }

        private void dgEqptPort_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        private void btnMapping_Click(object sender, RoutedEventArgs e)
        {

            PopupEqptPortMapping();
        }

        private void PopupEqptPortMapping()
        {
            string strEqptID = string.Empty;

            // 설비시트에 설비선택이 되어 있는 경우 해당설비를 넘겨줌. 아닌 경우 조회 콤보박스에 있는 설비 넘겨줌.
            if (_Util.GetDataGridCheckFirstRowIndex(dgEqptPort, "CHK") == -1)
            {
                if (cboEquipment.Text.Equals("ALL"))
                {
                    Util.MessageValidation("SFU1673"); //설비를 선택하세요
                    return;
                }
                else
                {
                    // 콤보박스에 설비가 ALL 이 아닌 경우 해당 설비로 등록
                    strEqptID = Convert.ToString(cboEquipment.SelectedValue);
                }
            }
            else
            {
                strEqptID = Util.NVC(DataTableConverter.GetValue(dgEqptPort.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEqptPort, "CHK")].DataItem, "EQPTID"));
            }

            PACK003_051_EQPT_PORT_MAPPING popupEqptPortMapping = new PACK003_051_EQPT_PORT_MAPPING { FrameOperation = FrameOperation };

            object[] parameters = new object[2];
            //if (Convert.ToString(cboEquipment.SelectedValue) != null && !string.IsNullOrEmpty(Convert.ToString(cboEquipment.SelectedValue)))
            //parameters[0] = Convert.ToString(cboEquipment.SelectedValue);
            if (strEqptID != null && !string.IsNullOrEmpty(strEqptID))
                parameters[0] = strEqptID;
            else
                parameters[0] = "";

            // 포트설정 팝업 화면에서 diffusion_site 사용 사이트 인 경우 드롭다운리스트로 설정 가능한 포트 조회하기 위함.
            parameters[1] = _DiffusionSiteFlag;

            C1WindowExtension.SetParameters(popupEqptPortMapping, parameters);
            popupEqptPortMapping.Closed += new EventHandler(PopupEqptPortMapping_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupEqptPortMapping.ShowModal()));

        }

        private void PopupEqptPortMapping_Closed(object sender, EventArgs e)
        {
            PACK003_051_EQPT_PORT_MAPPING popup = sender as PACK003_051_EQPT_PORT_MAPPING;

            if (popup != null && popup.DialogResult == MessageBoxResult.Cancel)
            {
                GetEqptPort();
            }
        }

        #region 투입 포트 설정 Tab
        private void cboInPortEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            INPORT_EQSGID = cboInPortEquipmentSegment.GetStringValue();

            string[] arrColumn3 = { "LANGID", "EQSGID" };
            string[] arrCondition3 = { LoginInfo.LANGID, INPORT_EQSGID };
            cboInPortEquipment.SetDataComboItem(_Da4, arrColumn3, arrCondition3, CommonCombo.ComboStatus.ALL, false);

            ClearInPortTabDataGrid();
        }

        private void cboInPortEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetEqptInPort();
            GetBachOrder(dgInPortBatchOrder, dgInPortMaterial);
        }

        private void btnInPortSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (cboEquipment.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1673"); //설비를 선택하세요
                return;
            }

            GetEqptInPort();
            GetBachOrder(dgInPortBatchOrder, dgInPortMaterial);
        }

        private void btnInPortUnMapping_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgEqptInPort, "CHK") == -1)
            {
                Util.MessageValidation("SFU3830"); //포트를 선택해주세요
                return;
            }

            SavePortInfo(dgEqptInPort
                      , null
                      , null
                      , "N"
                      , "DELETE");
        }

        private void btnInPortMapping_Click(object sender, RoutedEventArgs e)
        {
            PopupEqptInPortMapping();
        }

        private void PopupEqptInPortMapping()
        {
            string strEqptID = string.Empty;

            // 설비시트에 설비선택이 되어 있는 경우 해당설비를 넘겨줌. 아닌 경우 조회 콤보박스에 있는 설비 넘겨줌.
            if (_Util.GetDataGridCheckFirstRowIndex(dgEqptInPort, "CHK") == -1)
            {
                if (cboInPortEquipment.Text.Equals("ALL"))
                {
                    Util.MessageValidation("SFU1673"); //설비를 선택하세요
                    return;
                }
                else
                {
                    // 콤보박스에 설비가 ALL 이 아닌 경우 해당 설비로 등록
                    strEqptID = Convert.ToString(cboInPortEquipment.SelectedValue);
                }
            }
            else
            {
                strEqptID = Util.NVC(DataTableConverter.GetValue(dgEqptInPort.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEqptInPort, "CHK")].DataItem, "EQPTID"));
            }

            PACK003_051_EQPT_PORT_MAPPING popupEqptPortMapping = new PACK003_051_EQPT_PORT_MAPPING { FrameOperation = FrameOperation };

            object[] parameters = new object[2];
            //if (Convert.ToString(cboEquipment.SelectedValue) != null && !string.IsNullOrEmpty(Convert.ToString(cboEquipment.SelectedValue)))
            //parameters[0] = Convert.ToString(cboEquipment.SelectedValue);
            if (strEqptID != null && !string.IsNullOrEmpty(strEqptID))
                parameters[0] = strEqptID;
            else
                parameters[0] = "";

            // 포트설정 팝업 화면에서 diffusion_site 사용 사이트 인 경우 드롭다운리스트로 설정 가능한 포트 조회하기 위함.
            parameters[1] = _DiffusionSiteFlag;

            C1WindowExtension.SetParameters(popupEqptPortMapping, parameters);
            popupEqptPortMapping.Closed += new EventHandler(PopupEqptInPortMapping_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupEqptPortMapping.ShowModal()));

        }

        private void PopupEqptInPortMapping_Closed(object sender, EventArgs e)
        {
            PACK003_051_EQPT_PORT_MAPPING popup = sender as PACK003_051_EQPT_PORT_MAPPING;

            if (popup != null && popup.DialogResult == MessageBoxResult.Cancel)
            {
                GetEqptInPort();
            }
        }

        private void ClearInPortTabDataGrid()
        {
            dgEqptInPort.ClearRows();
            dgInPortBatchOrder.ClearRows();
            dgInPortMaterial.ClearRows();
        }

        private void GetEqptInPort()
        {
            try
            {
                if (_DiffusionSiteFlag)
                {
                    dgEqptInPort.Columns["EQPT_NAME"].Visibility = Visibility.Collapsed;
                }

                Util.gridClear(dgEqptInPort);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("EQPT_GROUP_ID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = INPORT_EQSGID;
                row["PROCID"] = null;
                row["EQPTID"] = cboInPortEquipment.Text.Equals("ALL") ? null : Convert.ToString(cboInPortEquipment.SelectedValue);
                row["EQPT_GROUP_ID"] = GetTabGroupName();
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync(_Da2, "RQSTDT", "RSLTDT", dt);
                Util.GridSetData(dgEqptInPort, result, FrameOperation, true);
                dgEqptInPort.MergingCells -= dgEqptInPort_MergingCells;
                dgEqptInPort.MergingCells += dgEqptInPort_MergingCells;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private string GetTabGroupName()
        {
            string result = string.Empty;

            if (this.tabControlMain.SelectedIndex.Equals(0))
            {
                result = "CMA,BMA,MCA";
            }
            else
            {
                result = "CNV";
            }

            return result;
        }

        private void dgEqptInPort_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (dgEqptInPort.Rows.Count <= 0)
                {
                    return;
                }
                int x = 0;
                int x1 = 0;
                for (int i = x1; i < dgEqptInPort.GetRowCount(); i++)
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dgEqptInPort.GetCell(x, dgEqptInPort.Columns["EQPTID"].Index).Value)))
                    {
                        if (Util.NVC(dgEqptInPort.GetCell(x, dgEqptInPort.Columns["EQPTID"].Index).Value) == Util.NVC(dgEqptInPort.GetCell(i, dgEqptInPort.Columns["EQPTID"].Index).Value))
                        {
                            x1 = i;
                        }
                        else
                        {
                            for (int j = 1; j < dgEqptInPort.Columns.Count - 6; j++)
                            {
                                // 2024.08.26 BY 최평부
                                if (j != 3 && j != 4)
                                {
                                    e.Merge(new DataGridCellsRange(dgEqptInPort.GetCell((int)x, (int)j), dgEqptInPort.GetCell((int)x1, (int)j)));
                                }
                            }

                            x = x1 + 1;
                            i = x1;
                        }
                    }

                }
                for (int j = 1; j < dgEqptInPort.Columns.Count - 6; j++)
                {
                    //최평부 추가 MERGE 컬럼 구분 2024.07.11
                    if (j != 3 && j != 4)
                    {
                        e.Merge(new DataGridCellsRange(dgEqptInPort.GetCell((int)x, (int)j), dgEqptInPort.GetCell((int)x1, (int)j)));
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion
    }
}