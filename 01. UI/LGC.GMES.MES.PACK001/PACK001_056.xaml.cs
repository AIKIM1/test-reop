/*************************************************************************************
  Created Date : 2019.12.09
       Creator : 손우석
   Description : 단동 설비 자재 이력조회 (Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2019.08.31 손우석 CSR ID 11100 자동차1동 Pilot 4호기 레진공정 모니터링 관련 [요청번호] C20191206-000278
  2020.02.20 손우석 이재호 - 폴란드 2동 PACK 1, 2 프로젝트 지원 요청 처리
  2020.03.16 이재호 서비스 번호 42040 조회조건 추가 및 EVENT_NAME 칼럼 추가 [요청번호] C20200316-000372
  2023.03.20 정용석 [E20230228-000004] - [생산PI] Resin loss관리를 위한 Resin잔량 Data수집 요청의 건 - 조회 Data Un Pivot 처리 제거
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_056 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private PACK001_056_DataHelper dataHelper = new PACK001_056_DataHelper();
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_056()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            this.dtpDateFrom.ApplyTemplate();
            this.dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-2);
            this.dtpDateTo.ApplyTemplate();
            this.dtpDateTo.SelectedDateTime = DateTime.Now;

            PackCommon.SetC1ComboBox(this.dataHelper.GetAreaInfo(), this.cboAreaID, true, "-SELECT-");
            this.cboAreaID.SelectedValue = LoginInfo.CFG_AREA_ID;
            this.Loaded -= this.UserControl_Loaded;
        }

        private void SearchProcess()
        {
            // Validation...
            if (this.dtpDateFrom.SelectedDateTime.Date > this.dtpDateTo.SelectedDateTime.Date)
            {
                Util.MessageValidation("SFU3569");  // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                this.dtpDateFrom.Focus();
                return;
            }

            TimeSpan timeSpan = this.dtpDateTo.SelectedDateTime.Date - this.dtpDateFrom.SelectedDateTime.Date;
            if (timeSpan.Days > 7)
            {
                Util.MessageValidation("SFU3567");
                return;
            }

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtRowCount, 0);
                Util.gridClear(this.dgResult);
                PackCommon.DoEvents();

                DateTime fromDate = this.dtpDateFrom.SelectedDateTime.Date;
                DateTime toDate = this.dtpDateTo.SelectedDateTime.Date;
                string equipmentSegmentID = this.cboEquipmentSegmentID.SelectedValue.ToString();
                string LOTID = this.txtLOTID.Text;

                DataTable dt = this.dataHelper.GetEquipmentDataCollectHistory(LOTID, fromDate, toDate, equipmentSegmentID);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtRowCount, dt.Rows.Count);
                    Util.GridSetData(dgResult, dt, FrameOperation);
                }
                else
                {
                    Util.MessageValidation("SFU2951");
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
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.SearchProcess();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void cboAreaID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string areaID = e.NewValue.ToString();
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(areaID), this.cboEquipmentSegmentID, "-SELECT-");
            this.cboEquipmentSegmentID.SelectedValue = LoginInfo.CFG_EQSG_ID;
            if (this.cboEquipmentSegmentID.SelectedValue == null)
            {
                this.cboEquipmentSegmentID.SelectedIndex = 0;
            }
        }
        #endregion
    }

    internal class PACK001_056_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_056_DataHelper()
        {

        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - 동코드 정보
        internal DataTable GetAreaInfo()
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_AREA_BY_AREATYPE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
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

        // 순서도 호출 - 단동 설비 자재 이력 조회
        internal DataTable GetEquipmentDataCollectHistory(string LOTID, DateTime fromDate, DateTime toDate, string equipmentSegmentID)
        {
            string bizRuleName = "DA_PRD_SEL_TB_SFC_EQPT_DATA_CLCT_BY_LOTID";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("CLCT_DTTM_START", typeof(string));
                dtRQSTDT.Columns.Add("CLCT_DTTM_END", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
                drRQSTDT["CLCT_DTTM_START"] = !string.IsNullOrEmpty(LOTID) ? null : fromDate.ToString("yyyyMMdd");
                drRQSTDT["CLCT_DTTM_END"] = !string.IsNullOrEmpty(LOTID) ? null : toDate.ToString("yyyyMMdd");
                drRQSTDT["EQSGID"] = !string.IsNullOrEmpty(LOTID) ? null : equipmentSegmentID;
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