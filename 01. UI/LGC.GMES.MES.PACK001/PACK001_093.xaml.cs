/*************************************************************************************
 Created Date : 2022.08.24
      Creator : 정용석
  Description : 레진 Purge 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.24  정용석    : Initial Created.
  2023.03.06  정용석    : [E20230228-000004] - [생산PI] Resin loss관리를 위한 Resin잔량 Data수집 요청의 건
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_093 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private PACK001_093_DataHelper dataHelper = new PACK001_093_DataHelper();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_093()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        // 초기화
        private void Initialize()
        {
            try
            {
                this.dtpFromDate.ApplyTemplate();
                this.dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-2);
                this.dtpToDate.ApplyTemplate();
                this.dtpToDate.SelectedDateTime = DateTime.Now;
                PackCommon.SetC1ComboBox(this.dataHelper.GetAreaInfo(), this.cboAreaID);
                PackCommon.SetC1ComboBox(this.dataHelper.GetResinCollectItemGroupCodeInfo(), this.cboCollectItemGroupCode, true, "-ALL-");
                PackCommon.SearchRowCount(ref this.txtRowCount, 0);

                this.cboAreaID.SelectedValue = LoginInfo.CFG_AREA_ID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 조회
        private void SearchProcess()
        {
            // Validation Check...
            if (this.dtpFromDate.SelectedDateTime.Date > this.dtpToDate.SelectedDateTime.Date)
            {
                Util.MessageValidation("SFU3569");  // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                this.dtpFromDate.Focus();
                return;
            }

            TimeSpan timeSpan = this.dtpToDate.SelectedDateTime.Date - this.dtpFromDate.SelectedDateTime.Date;
            if (timeSpan.Days > 7)
            {
                Util.MessageValidation("SFU3567");
                return;
            }

            // Search Process
            try
            {
                PackCommon.SearchRowCount(ref this.txtRowCount, 0);
                Util.gridClear(this.dgLOTList);
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();

                DateTime dteFromDate = this.dtpFromDate.SelectedDateTime.Date;
                DateTime dteToDate = this.dtpToDate.SelectedDateTime.Date.AddDays(1);
                string areaID = this.cboAreaID.SelectedValue.ToString();
                string equipmentSegmentID = this.cboEquipmentSegmentID.SelectedValue.ToString();
                string equipmentID = this.txtEquipmentID.Text;
                string processID = this.cboProcessID.SelectedValue.ToString();
                string clctItem = this.txtClctItem.Text;
                string eventName = this.cboCollectItemGroupCode.SelectedValue == null ? null : this.cboCollectItemGroupCode.SelectedValue.ToString();
                string materialID = this.txtMaterialID.Text;

                DataTable dt = this.dataHelper.GetResinPurgeHistory(dteFromDate, dteToDate, areaID, equipmentSegmentID, equipmentID, processID, clctItem, eventName, materialID);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtRowCount, dt.Rows.Count);
                    Util.GridSetData(this.dgLOTList, dt, FrameOperation);
                }
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
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void cboAreaID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string areaID = e.NewValue.ToString();
            DataTable dt = this.dataHelper.GetEquipmentSegmentInfo(areaID);
            PackCommon.SetC1ComboBox(dt, this.cboEquipmentSegmentID, true, "-ALL-");
        }

        private void cboEquipmentSegmentID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string areaID = this.cboAreaID.SelectedValue.ToString();
            string equipmentSegmentID = e.NewValue.ToString();
            PackCommon.SetC1ComboBox(this.dataHelper.GetProcessInfo(equipmentSegmentID), this.cboProcessID, true, "-ALL-");
        }
        #endregion
    }

    internal class PACK001_093_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_093_DataHelper()
        {

        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - 동코드 정보
        internal DataTable GetAreaInfo()
        {
            string bizRuleName = "DA_BAS_SEL_AREA_BY_AREATYPE_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - EquipmentSegment 정보
        internal DataTable GetEquipmentSegmentInfo(string areaID)
        {
            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EXCEPT_GROUP"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 공정정보
        internal DataTable GetProcessInfo(string equipmentSegmentID)
        {
            string bizRuleName = "DA_BAS_SEL_PROCESS_PACK_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 수집항목그룹 정보
        internal DataTable GetResinCollectItemGroupCodeInfo()
        {
            string bizRuleName = "DA_PRD_SEL_CLCT_GR_CODE";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            // Event Name은 Resin 씨리즈와 PPURGIN만 선택
            return dtRSLTDT.AsEnumerable().Where(x => x.Field<string>("CLCT_GR_CODE").Contains("RESIN") || x.Field<string>("CLCT_GR_CODE").Equals("PPURGIN")).OrderBy(x => x.Field<string>("CLCT_GR_CODE")).CopyToDataTable();
        }

        // 순서도 호출 - 레진 Purge 이력 조회
        internal DataTable GetResinPurgeHistory(DateTime dteFromDate, DateTime dteToDate, string areaID, string equipmentSegmentID, string equipmentID, string processID, string clctItem, string eventName, string materialID)
        {
            string bizRuleName = "DA_PRD_SEL_TB_SFC_EQPT_DATA_CLCT_LIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("EQPTID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("CLCTITEM", typeof(string));
                dtRQSTDT.Columns.Add("EVENT_NAME", typeof(string));
                dtRQSTDT.Columns.Add("MTRLID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_CLCT_DTTM", typeof(DateTime));
                dtRQSTDT.Columns.Add("TO_CLCT_DTTM", typeof(DateTime));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = string.IsNullOrEmpty(areaID) ? null : areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(equipmentSegmentID) || equipmentSegmentID.Equals(true)) ? null : equipmentSegmentID;
                drRQSTDT["EQPTID"] = string.IsNullOrEmpty(equipmentID) ? null : equipmentID;
                drRQSTDT["PROCID"] = string.IsNullOrEmpty(processID) ? null : processID;
                drRQSTDT["CLCTITEM"] = string.IsNullOrEmpty(clctItem) ? null : clctItem;
                drRQSTDT["EVENT_NAME"] = string.IsNullOrEmpty(eventName) ? null : eventName;
                drRQSTDT["MTRLID"] = string.IsNullOrEmpty(materialID) ? null : materialID;
                drRQSTDT["FROM_CLCT_DTTM"] = dteFromDate;
                drRQSTDT["TO_CLCT_DTTM"] = dteToDate;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }
        #endregion
    }
}