/*************************************************************************************
 Created Date : 2017.12.08
      Creator : 
   Decription : CST 관리
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.08  DEVELOPER : Initial Created.
  2023.07.03 이윤중 CSR ID E20230627-000461 2023-07-01 Loss 체계 개선 이전 날짜 선택 불가 로직 추가 (임시)
  2023.07.06 이윤중 CSR ID E20230627-000461 설비 Loss 변경 제한 기준 날짜 공통코드 추가 (EQPT_LOSS_CODE_APPLY_AREA - ATTRIBUTE5 - 2023-07-01 00:00:00.000)
  2024.08.07 김동일 [E20240428-002005] - AZS-Stacking 설비의 경우 Machine Level 로 조회 가능 하도록 기능 추가
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_126 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        DateTime dBaseDate = new DateTime();

        private string _Machine_Use_Chk_Flag = string.Empty;

        public COM001_126()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            initCombo();

            SetGridCboItem();

            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);


        }

        #endregion

        #region Initialize
        private void initCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboEquipment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = {  cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = {  cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);

            //2023-07-03 - 설비 Loss 체계 개편 관련 제한 로직 추가 - yjlee
            ldpDatePickerFrom.SelectedDataTimeChanged += ldpDatePickerFrom_SelectedDataTimeChanged;
            ldpDatePickerTo.SelectedDataTimeChanged += ldpDatePickerTo_SelectedDataTimeChanged;

            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;

            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
        }


        #endregion

        #region Funct

        private void SearchData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("WRK_DATE_FROM", typeof(string));
                dt.Columns.Add("WRK_DATE_TO", typeof(string));

                DataRow dr = dt.NewRow();
                dr["AREAID"] = cboArea.SelectedValue.ToString();

                if (_Machine_Use_Chk_Flag.Equals("Y") &&
                    _MACHINE_CTRL.Visibility == Visibility.Visible && chkMachineLevel?.IsChecked == true &&
                    _MACHINE_CBO_CTRL.Visibility == Visibility.Visible)
                    dr["EQPTID"] = Util.NVC(cboEquipment_Machine.SelectedValue);
                else
                    dr["EQPTID"] = cboEquipment.SelectedValue.ToString();

                //dr["WRK_DATE_FROM"] = ldpDatePickerFrom.SelectedDateTime.ToShortDateString().Replace("-", "");
                //dr["WRK_DATE_TO"] = ldpDatePickerTo.SelectedDateTime.ToShortDateString().Replace("-", "");
                dr["WRK_DATE_FROM"] = ldpDatePickerFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["WRK_DATE_TO"] = ldpDatePickerTo.SelectedDateTime.ToString("yyyyMMdd");

                dt.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTLOSS_SM", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }


                        Util.GridSetData(dgLossList, bizResult, FrameOperation, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

              //  DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SM", "RQSTDT", "RSLTDT", dt);



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SetGridCboItem()
        {
            try
            {
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPTLOSS_EIOSTAT", "RQSTDT", "RSLTDT", null);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    (dgLossList.Columns["EIOSTAT"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE", typeof(string));
                dt.Columns.Add("CBO_NAME", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CBO_CODE"] = "EQP";
                dr["CBO_NAME"] = "EQP";
                dt.Rows.Add(dr);

                DataRow dr2 = dt.NewRow();
                dr2["CBO_CODE"] = "MAN";
                dr2["CBO_NAME"] = "MAN";
                dt.Rows.Add(dr2);

                (dgLossList.Columns["SRC_TYPE_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetMachineEqptCombo(C1ComboBox cbo)
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
            dr["AREAID"] = Util.NVC(cboArea.SelectedValue).Equals("") ? null : cboArea.SelectedValue.ToString();
            dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue).Equals("") ? null : cboEquipmentSegment.SelectedValue.ToString();
            dr["PROCID"] = Util.NVC(cboProcess.SelectedValue).Equals("") ? null : cboProcess.SelectedValue.ToString();
            dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue).Equals("") ? null : cboEquipment.SelectedValue.ToString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;

        }

        public void MachineEqpt_Loss_Modify_Chk()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dr["COM_TYPE_CODE"] = "EQPTLOSS_MACHINE_EQPT_MODIFY_PROCESS";
                dr["COM_CODE"] = Util.NVC(cboProcess.SelectedValue);
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    _Machine_Use_Chk_Flag = "Y";
                    _MACHINE_CTRL.Visibility = Visibility.Visible;                    
                }
                else
                {
                    _Machine_Use_Chk_Flag = string.Empty;
                    _MACHINE_CTRL.Visibility = Visibility.Collapsed;
                    _MACHINE_CBO_CTRL.Visibility = Visibility.Collapsed;
                    chkMachineLevel.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                _Machine_Use_Chk_Flag = string.Empty;
                _MACHINE_CTRL.Visibility = Visibility.Collapsed;
                _MACHINE_CBO_CTRL.Visibility = Visibility.Collapsed;
                chkMachineLevel.IsChecked = false;

                Util.MessageException(ex);
            }
        }
        
        #endregion

        #region Event


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            try
            {
                DataSet ds = new DataSet();

                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("LOSS_SEQNO", typeof(Int32));
                dt.Columns.Add("STRT_DTTM_YMDHMS", typeof(string));
                dt.Columns.Add("EIOSTAT", typeof(string));
                dt.Columns.Add("TRBL_CODE", typeof(string));
                dt.Columns.Add("LOSS_CODE", typeof(string));
                dt.Columns.Add("LOSS_DETL_CODE", typeof(string));
                dt.Columns.Add("LOSS_NOTE", typeof(string));
                dt.Columns.Add("SRC_TYPE_CODE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));



                for (int i = 0; i < dgLossList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        DataRow dr = dt.NewRow();
                        dr["AREAID"] = cboArea.SelectedValue.ToString();
                        dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "EQPTID"));
                        dr["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_SEQNO"));
                        dr["STRT_DTTM_YMDHMS"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "STRT_DTTM_YMDHMS"));
                        dr["EIOSTAT"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "EIOSTAT"));
                        dr["TRBL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "TRBL_CODE")).Equals("") ? "0000" : Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "TRBL_CODE"));
                        dr["LOSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_CODE")).Equals("") ? null : Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_CODE"));
                        dr["LOSS_DETL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_DETL_CODE")).Equals("") ? null : Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_DETL_CODE"));
                        dr["LOSS_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_NOTE")).Equals("") ? null : Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_NOTE"));
                        dr["SRC_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "SRC_TYPE_CODE"));
                        dr["USERID"] = LoginInfo.USERID;
                        dt.Rows.Add(dr);

                    }
                }
                ds.Tables.Add(dt);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_UPD_SM", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1731");

                        SearchData();

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


        private void ldpDatePickerFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //2023.07.06 - 설비 Loss 등록 제한 기준 날짜 공통코드 추가 (EQPT_LOSS_CODE_APPLY_AREA - ATTRIBUTE5 - 2023-07-01 00:00:00.000)
            string sAreaId = cboArea.SelectedValue.ToString();
            dBaseDate = Util.LossDataUnalterable_BaseDate(sAreaId);

            if (dBaseDate != null
                && ldpDatePickerFrom.SelectedDateTime < dBaseDate)
            {
                Util.MessageValidation("SFU9040", dBaseDate.ToString("yyyy-MM-dd")); // 설비Loss 체계 개편에 따라, 7월 이전 설비Loss 등록이 불가합니다. 
                ldpDatePickerFrom.SelectedDateTime = dBaseDate;
                return;
            }
        }

        private void ldpDatePickerTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //2023.07.06 - 설비 Loss 등록 제한 기준 날짜 공통코드 추가 (EQPT_LOSS_CODE_APPLY_AREA - ATTRIBUTE5 - 2023-07-01 00:00:00.000)
            string sAreaId = cboArea.SelectedValue.ToString();
            dBaseDate = Util.LossDataUnalterable_BaseDate(sAreaId);

            if (dBaseDate != null
                && ldpDatePickerTo.SelectedDateTime < dBaseDate)
            {
                Util.MessageValidation("SFU9040", dBaseDate.ToString("yyyy-MM-dd")); // 설비Loss 체계 개편에 따라, 7월 이전 설비Loss 등록이 불가합니다. 
                ldpDatePickerTo.SelectedDateTime = dBaseDate;
                return;
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sAreaId = cboArea.SelectedValue.ToString();
            dBaseDate = Util.LossDataUnalterable_BaseDate(sAreaId);
            if (dBaseDate != null
                && ldpDatePickerFrom.SelectedDateTime < dBaseDate)
            {
                ldpDatePickerFrom.SelectedDateTime = dBaseDate;
            }

            if(dBaseDate != null
                && ldpDatePickerTo.SelectedDateTime < dBaseDate)
            {
                ldpDatePickerTo.SelectedDateTime = dBaseDate;
            }
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (Util.NVC(cboProcess.SelectedValue).Equals("")) return;

                MachineEqpt_Loss_Modify_Chk();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (Util.NVC(cboEquipment.SelectedValue).Equals("")) return;

                if (_Machine_Use_Chk_Flag.Equals("Y"))
                {
                    SetMachineEqptCombo(cboEquipment_Machine);
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkMachineLevel_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Machine_Use_Chk_Flag.Equals("Y"))
                    _MACHINE_CBO_CTRL.Visibility = Visibility.Visible;
                else
                    _MACHINE_CBO_CTRL.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkMachineLevel_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _MACHINE_CBO_CTRL.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
        
    }
}

