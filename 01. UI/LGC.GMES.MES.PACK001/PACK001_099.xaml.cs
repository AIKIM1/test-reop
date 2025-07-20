/*************************************************************************************
 Created Date : 2023.10.01
      Creator : 정용석
  Description : 생산실적 SMS 전송
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.01 정용석 : Initial Created. (조립동의 SMS 전송화면 참조)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_099 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private PACK001_099_DataHelper dataHelper = new PACK001_099_DataHelper();
        private string selectedEquipmentSegmentID = string.Empty;
        private string selectedProcessID = string.Empty;
        private string smsmGroupCode = "SMS_MI_PACK_001";           // MMD SMS 그룹 관리에 있는 SMS 그룹 ID 값
        private DataTable dtDeletedPhoneList = new DataTable();     // 라인, 공정별 전화번호 저장 리스트
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_099()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            this.dtpDateMonth.SelectedDateTime = DateTime.Now;
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboEquipmentSegment, true, "SELECT");
            PackCommon.SetC1ComboBox(this.dataHelper.GetProcessInfo(), this.cboProcess, true, "SELECT");
        }

        // 조회질
        private void SearchProcess()
        {
            // Validation Check...
            if (string.IsNullOrEmpty(this.cboEquipmentSegment.SelectedValue.ToString()) || this.cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.MessageInfo("SFU1223");    // 라인을 선택하세요.
                return;
            }

            if (string.IsNullOrEmpty(this.cboProcess.SelectedValue.ToString()) || this.cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.MessageInfo("SFU1459");    // 공정을 선택하세요.
                return;
            }

            try
            {
                DateTime calDate = this.dtpDateMonth.SelectedDateTime;
                string equipmentSegmentID = this.cboEquipmentSegment.SelectedValue.ToString();
                string processID = this.cboProcess.SelectedValue.ToString();

                this.loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(this.dgList);
                Util.gridClear(this.dgSMSTargetPhoneList);
                this.txtPhoneNo.Text = string.Empty;
                this.dtDeletedPhoneList.Clear();
                PackCommon.DoEvents();
                DataSet ds = this.dataHelper.GetOutputCompareToPlan(this.smsmGroupCode, calDate, equipmentSegmentID, processID);
                if (CommonVerify.HasTableInDataSet(ds))
                {
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA"]))
                    {
                        Util.GridSetData(this.dgList, ds.Tables["OUTDATA"], FrameOperation);
                    }
                    if (CommonVerify.HasTableRow(ds.Tables["OUT_CHARGE_USER_PHONE_NO"]))
                    {
                        Util.GridSetData(this.dgSMSTargetPhoneList, ds.Tables["OUT_CHARGE_USER_PHONE_NO"], FrameOperation);
                        this.dtDeletedPhoneList = new DataTable();
                        this.dtDeletedPhoneList = ds.Tables["OUT_CHARGE_USER_PHONE_NO"].Clone();
                    }
                }

                this.selectedEquipmentSegmentID = equipmentSegmentID;
                this.selectedProcessID = processID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 전화번호 저장
        private void SaveProcess()
        {
            try
            {
                // 조회를 먼저 해야지 등록가능하도록 함.
                if (string.IsNullOrEmpty(this.selectedEquipmentSegmentID) || string.IsNullOrEmpty(this.selectedProcessID))
                {
                    Util.MessageValidation("SFU8485");
                    return;
                }

                // 등록된 데이터 없으면 Return
                if (this.dgSMSTargetPhoneList.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU8501", ObjectDic.Instance.GetObjectName("전화번호"));
                    return;
                }

                // 전화번호 저장
                PackCommon.DoEvents();
                List<string> lstPhoneList = DataTableConverter.Convert(this.dgSMSTargetPhoneList.ItemsSource).AsEnumerable().Select(x => x.Field<string>("CHARGE_USER_PHONE_NO")).ToList();
                if (this.dataHelper.SetPhoneList(this.selectedEquipmentSegmentID, this.selectedProcessID, this.smsmGroupCode, lstPhoneList, this.dtDeletedPhoneList))
                {
                    Util.MessageInfo("SFU1270");        // 저장되었습니다.
                    this.SearchProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 전화번호 추가
        private void AddPhoneNo()
        {
            // 조회를 먼저 해야지 등록가능하도록 함.
            if (string.IsNullOrEmpty(this.selectedEquipmentSegmentID) || string.IsNullOrEmpty(this.selectedProcessID))
            {
                Util.MessageValidation("SFU8485");
                return;
            }

            // 그냥 더하기만 눌렀을 경우
            if (string.IsNullOrEmpty(this.txtPhoneNo.Text))
            {
                Util.MessageValidation("SFU3179");
                this.txtPhoneNo.Select(0, this.txtPhoneNo.Text.Length);
                return;
            }

            // 전화번호 체크
            if (!CommonVerify.IsValidPhoneNumber(this.txtPhoneNo.Text))
            {
                Util.MessageValidation("SFU3179");
                this.txtPhoneNo.Select(0, this.txtPhoneNo.Text.Length);
                return;
            }

            // 전화번호 중복 체크
            if (DataTableConverter.Convert(this.dgSMSTargetPhoneList.ItemsSource).AsEnumerable().Where(x => x.Field<string>("CHARGE_USER_PHONE_NO") == this.txtPhoneNo.Text).Count() > 0)
            {
                Util.MessageValidation("SFU3179");
                this.txtPhoneNo.Select(0, this.txtPhoneNo.Text.Length);
                return;
            }

            DataTable dt = Util.MakeDataTable(this.dgSMSTargetPhoneList, true);
            DataRow dr = dt.NewRow();
            dr["SMS_GR_ID"] = this.smsmGroupCode;
            dr["EQSGID"] = this.selectedEquipmentSegmentID;
            dr["PROCID"] = this.selectedProcessID;
            dr["USE_FLAG"] = "Y";
            dr["SEND_USER_FLAG"] = "N";
            dr["CHARGE_USER_PHONE_NO"] = this.txtPhoneNo.Text;
            dt.Rows.Add(dr);
            this.dgSMSTargetPhoneList.ItemsSource = DataTableConverter.Convert(dt);

            // 삭제된거 다시 등록했을 경우, 삭제 리스트는 지우기 (조회시 A전화번호 존재 -> A전화번호 삭제 -> A전화번호 등록했을 경우)
            if (CommonVerify.HasTableRow(this.dtDeletedPhoneList))
            {
                var query = this.dtDeletedPhoneList.AsEnumerable().Where(x => x.Field<string>("CHARGE_USER_PHONE_NO") == this.txtPhoneNo.Text);
                if (query.Count() > 0)
                {
                    this.dtDeletedPhoneList.AsEnumerable().Where(x => x.Field<string>("CHARGE_USER_PHONE_NO") == this.txtPhoneNo.Text).ToList<DataRow>().ForEach(r => r.Delete());
                    this.dtDeletedPhoneList.AcceptChanges();
                }
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.SaveProcess();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.AddPhoneNo();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            C1.WPF.DataGrid.DataGridRow dataGridRow = new C1.WPF.DataGrid.DataGridRow();
            IList<FrameworkElement> ilist = button.GetAllParents();

            foreach (var item in ilist)
            {
                DataGridRowPresenter dataGridRowPresenter = item as DataGridRowPresenter;
                if (dataGridRowPresenter != null)
                {
                    dataGridRow = dataGridRowPresenter.Row;
                }
            }

            if ((((DataRowView)dataGridRow.DataItem).Row.RowState.GetString().ToUpper() != "ADDED"))
            {
                // 조회된 전화번호 리스트 중에 하나를 삭제했을 경우
                DataRow drDeletedPhoneList = this.dtDeletedPhoneList.NewRow();
                drDeletedPhoneList["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "EQSGID"));
                drDeletedPhoneList["PROCID"] = Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "PROCID"));
                drDeletedPhoneList["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "EQPTID"));
                drDeletedPhoneList["USE_FLAG"] = "N";
                drDeletedPhoneList["SEND_USER_FLAG"] = Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "SEND_USER_FLAG"));
                drDeletedPhoneList["CHARGE_USER_PHONE_NO"] = Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "CHARGE_USER_PHONE_NO"));
                this.dtDeletedPhoneList.Rows.Add(drDeletedPhoneList);
            }

            this.dgSMSTargetPhoneList.SelectedItem = dataGridRow.DataItem;
            this.dgSMSTargetPhoneList.RemoveRow(dataGridRow.Index);
        }

        private void txtPhoneNo_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
            {
                return;
            }
            this.AddPhoneNo();
        }
        #endregion
    }

    internal class PACK001_099_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_099_DataHelper()
        {

        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - CommonCode 정보
        internal DataTable GetCommonCodeInfo(string cmcdType)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - EquipmentSegment 정보
        internal DataTable GetEquipmentSegmentInfo(string areaID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EXCEPT_GROUP"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 공정 정보
        internal DataTable GetProcessInfo()
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_PROCESS_PACK_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
                drRQSTDT["PCSGID"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.AsEnumerable().Where(x => x.Field<string>("CBO_CODE") == "P5000" || x.Field<string>("CBO_CODE") == "P9000").CopyToDataTable();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 일별, SHIFT별, 시간별 계획 대비 생산실적 조회
        internal DataSet GetOutputCompareToPlan(string smsGroupCode, DateTime calDate, string equipmentSegmentID, string processID)
        {
            DataSet dsINDATA = new DataSet();
            DataSet dsResult = new DataSet();

            try
            {
                string bizRuleName = "BR_PRD_GET_OUTPUT_COMPLARE_PLAN_FOR_PACK";
                string outDataSetName = "OUTDATA,OUT_CHARGE_USER_PHONE_NO";

                DataTable INDATA = new DataTable("INDATA");
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SMS_GR_ID", typeof(string));
                INDATA.Columns.Add("CALDATE", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SMS_GR_ID"] = smsGroupCode;
                drINDATA["CALDATE"] = calDate.ToString("yyyyMM");
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["EQSGID"] = equipmentSegmentID;
                drINDATA["PROCID"] = processID;
                INDATA.Rows.Add(drINDATA);

                dsINDATA.Tables.Add(INDATA);


                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(dt => dt.TableName).ToList());
                DataSet dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA, null);
                if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    dsResult = dsOUTDATA.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dsResult;
        }

        // 순서도 호출 - 라인, 공정별 SMS 발송 전화번호 저장
        internal bool SetPhoneList(string equipmentSegmentID, string processID, string smsGroupCode, List<string> lstPhoneNo, DataTable dtDeletedPhoneList)
        {
            bool returnValue = false;
            string bizRuleName = "BR_PRD_REG_CHARGE_PHONE_NO_FOR_PACK";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = string.Empty;

            try
            {
                // INDATA (FOR TB_MMD_SMS_GR_EPQT)
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SMS_GR_ID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("USE_FLAG", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SMS_GR_ID"] = smsGroupCode;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["EQSGID"] = equipmentSegmentID;
                drINDATA["PROCID"] = processID;
                drINDATA["USE_FLAG"] = "Y";
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);

                // IN_CHARGE_USER_PHONE_NO (FOR TB_MMD_EQPT_CHARGE_PHONE_NO)
                DataTable dtIN_CHARGE_USER_PHONE_NO = new DataTable("IN_CHARGE_USER_PHONE_NO");
                dtIN_CHARGE_USER_PHONE_NO.Columns.Add("CHARGE_USER_PHONE_NO", typeof(string));
                dtIN_CHARGE_USER_PHONE_NO.Columns.Add("USE_FLAG", typeof(string));
                dtIN_CHARGE_USER_PHONE_NO.Columns.Add("SEND_USER_FLAG", typeof(string));

                // 사용여부가 Y인 놈들.
                foreach (string phoneNo in lstPhoneNo)
                {
                    DataRow drIN_CHARGE_USER_PHONE_NO = dtIN_CHARGE_USER_PHONE_NO.NewRow();
                    drIN_CHARGE_USER_PHONE_NO["CHARGE_USER_PHONE_NO"] = phoneNo;
                    drIN_CHARGE_USER_PHONE_NO["USE_FLAG"] = "Y";
                    drIN_CHARGE_USER_PHONE_NO["SEND_USER_FLAG"] = "N";
                    dtIN_CHARGE_USER_PHONE_NO.Rows.Add(drIN_CHARGE_USER_PHONE_NO);
                }

                // 사용여부가 N인 놈들.
                foreach (DataRowView dataRowView in dtDeletedPhoneList.AsDataView())
                {
                    DataRow drIN_CHARGE_USER_PHONE_NO = dtIN_CHARGE_USER_PHONE_NO.NewRow();
                    drIN_CHARGE_USER_PHONE_NO["CHARGE_USER_PHONE_NO"] = dataRowView["CHARGE_USER_PHONE_NO"];
                    drIN_CHARGE_USER_PHONE_NO["USE_FLAG"] = dataRowView["USE_FLAG"];
                    drIN_CHARGE_USER_PHONE_NO["SEND_USER_FLAG"] = dataRowView["SEND_USER_FLAG"];
                    dtIN_CHARGE_USER_PHONE_NO.Rows.Add(drIN_CHARGE_USER_PHONE_NO);
                }

                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtIN_CHARGE_USER_PHONE_NO);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
                returnValue = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }
        #endregion
    }
}